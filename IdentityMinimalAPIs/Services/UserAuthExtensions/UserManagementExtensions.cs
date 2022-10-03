using IdentityMinimalAPIs.DTOs;
using IdentityMinimalAPIs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi.Models;
using SharedIdentity.Abstractions;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace IdentityMinimalAPIs.Services.UserAuthExtensions
{
    public static class UserManagementExtensions
    {
        public static WebApplication MapUserRegistrationEndpoints(this WebApplication app)
        {
            var accountRouter = app.MapGroup("/account").WithTags("User Account Endpoints");

            accountRouter.MapPost("/signup", async (
                UserRequestDTO userDTO,
                UserManager<User> userManager,
                IEmailServiceProvider emailService,
                IUserStore<User> userStore) =>
            {

                var user = new User
                {
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    Email = userDTO.Email,
                };

                if (userDTO.Enabled2fa)
                {
                    user.TwoFactorEnabled = true;
                }
                await userStore.SetUserNameAsync(user, user.Email, CancellationToken.None);

                var identityResult = await userManager.CreateAsync(user, userDTO.Password);

                if (identityResult.Succeeded)
                {
                    var userId = await userManager.GetUserIdAsync(user);
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    //Either send an email now or call the send email confirmation endpoint. That's why I have emailconfirmation endpoint. 
                    await SendEmailConfirmationCode(emailService, userEmailAddress: user.Email, userId: userId, code: code);

                    var available2Fas = await userManager.GetValidTwoFactorProvidersAsync(user);

                    return Results.Ok(new UserResponseDTO
                    {
                        UserId = userId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        EmailConfirmed = false,
                        EmailConfirmationCode = code,
                        Available2fas = available2Fas,
                    });
                }

                return Results.UnprocessableEntity(new AuthResultDTO
                {
                    Succeeded = false,
                    StatusCode = 422,
                    Message = "Unable to sign up at the moment",
                    TimeStamp = DateTime.Now,
                });
            })
                .Accepts<UserRequestDTO>("application/json")
                .Produces<UserResponseDTO>(StatusCodes.Status200OK)
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity);


            accountRouter.MapPost("/enable2fa", async (EnableMultiFactorDTO multiFactorDTO, UserManager<User> userManager, SignInManager<User> signInManager) =>
            {
                var user = await userManager.FindByIdAsync(multiFactorDTO.UserId);

                if (user == null)
                {
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 422,
                        Message = "User not found",
                        TimeStamp = DateTime.Now,
                    });
                }

                user.TwoFactorEnabled = true;
                user.Preferred2fa = multiFactorDTO.Selected2fa;
                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Results.Ok(new
                    {
                        preferred2fa = user.Preferred2fa,
                        TimeStamp = DateTime.Now,
                    });
                }

                return Results.UnprocessableEntity(new AuthResultDTO
                {
                    Succeeded = false,
                    StatusCode = 422,
                    Message = "Unable to enable two factor authentication",
                    TimeStamp = DateTime.Now,
                });
            })
                .Accepts<EnableMultiFactorDTO>("application/json")
                .Produces(StatusCodes.Status200OK)
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity)
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);


            accountRouter.MapPost("/disable2fa", async (EnableMultiFactorDTO multiFactorDTO, UserManager<User> userManager, SignInManager<User> signInManager) =>
            {
                var user = await userManager.FindByIdAsync(multiFactorDTO.UserId);

                if (user == null)
                {
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 422,
                        Message = "User not found",
                        TimeStamp = DateTime.Now,
                    });
                }
                user.TwoFactorEnabled = false;
                user.Preferred2fa = null;

                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var mfaProviders = await signInManager.UserManager.GetValidTwoFactorProvidersAsync(user);
                    return Results.Ok(new { MfaProviders = mfaProviders });
                }

                return Results.UnprocessableEntity(new AuthResultDTO
                {
                    Succeeded = false,
                    StatusCode = 422,
                    Message = "Unable to disable two factor authentication",
                    TimeStamp = DateTime.Now,
                });
            })
                .Accepts<EnableMultiFactorDTO>("application/json")
                .Produces(StatusCodes.Status200OK)
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity)
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);

            accountRouter.MapGet("/loadAuthenticatorData/{userId}", async (string userId, UserManager<User> userManager, SignInManager<User> signInManager, UrlEncoder urlEncoder) =>
            {
                var user = await userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "User not found",
                        TimeStamp = DateTime.Now,
                    });
                }

                if (user.Preferred2fa == TwoFactorProviderConstants.AuthenticatorProvider)
                {
                    var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
                    if (string.IsNullOrEmpty(unformattedKey))
                    {
                        await userManager.ResetAuthenticatorKeyAsync(user);
                        unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
                    }

                    var formattedKey = FormatKey(unformattedKey!);
                    var authenticatorUri = GenerateQrCodeUri(user.Email!, unformattedKey!, urlEncoder);

                    return Results.Ok(new
                    {
                        Preferred2Fa = TwoFactorProviderConstants.AuthenticatorProvider,
                        AuthenticatorCode = formattedKey,
                        AuthenticatorUrl = authenticatorUri
                    });
                }

                return Results.UnprocessableEntity(new AuthResultDTO
                {
                    Succeeded = false,
                    StatusCode = 422,
                    Message = "Unable to set the preferred two factor authentication type",
                    TimeStamp = DateTime.Now,
                });

            })
                .RequireAuthorization()
                .Accepts<Select2faDTO>("application/json")
                .Produces<AuthResultDTO>(StatusCodes.Status200OK)
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity)
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);

            accountRouter.MapPost("/verifyAuthenticatorApp", async (TwoFactorAuthRequestDTO twoFactorAuthRequestDTO, UserManager<User> userManger) =>
            {

                var code = twoFactorAuthRequestDTO.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
                var user = await userManger.FindByEmailAsync(twoFactorAuthRequestDTO.Email);

                if (user == null)
                {
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "User Not found",
                        TimeStamp = DateTime.Now,
                    });
                }

                var is2faCodeValid = await userManger.VerifyTwoFactorTokenAsync(user, tokenProvider: TwoFactorProviderConstants.AuthenticatorProvider, token: code);

                if (!is2faCodeValid)
                {
                    return Results.UnprocessableEntity(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 422,
                        Message = "Your verification code is invalid",
                        TimeStamp = DateTime.Now,
                    });

                }

                return Results.Ok(new AuthResultDTO
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Message = "Successfully verified your authenticator app",
                    TimeStamp = DateTime.Now,

                });

            })
                .RequireAuthorization()
                .Accepts<TwoFactorAuthRequestDTO>("application/json")
                .Produces<AuthResultDTO>()
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity)
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);


            accountRouter.MapGet("/recoveryCodes/{userId}", async (string userId, UserManager<User> userManager) =>
            {
                var user = await userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "User Not found",
                        TimeStamp = DateTime.Now,
                    });
                }

                var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                if (codes != null)
                {
                    return Results.Ok(new { Succeeded = true, RecoveryCodes = codes.ToArray() });
                }

                return Results.UnprocessableEntity(new AuthResultDTO
                {
                    Succeeded = false,
                    StatusCode = 422,
                    Message = "Unable to generate recovery codes",
                    TimeStamp = DateTime.Now,
                });

            })
                .RequireAuthorization()
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity)
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);


            accountRouter.MapPost("/confirmEmail", async (
                EmailConfirmationDTO emailConfirmationDTO,
                UserManager<User> userManager,
                IEmailServiceProvider emailServiceProvider) =>
            {
                if (emailConfirmationDTO.ConfirmationCode == null || emailConfirmationDTO.UserId == null)
                {
                    return Results.UnprocessableEntity(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 422,
                        Message = "Unable to verify your email",
                        TimeStamp = DateTime.Now,
                    });
                }

                var user = await userManager.FindByIdAsync(emailConfirmationDTO.UserId);

                if (user is null)
                {
                    return Results.UnprocessableEntity(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "User not found",
                        TimeStamp = DateTime.Now,
                    });
                }

                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(emailConfirmationDTO.ConfirmationCode));
                var result = await userManager.ConfirmEmailAsync(user, code);

                if (result.Succeeded)
                {

                    return Results.Ok(new UserResponseDTO
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        EmailConfirmed = user.EmailConfirmed,
                        TwoFactorEnabled = user.TwoFactorEnabled,
                    });
                }

                return Results.UnprocessableEntity();
            })
                .Accepts<EmailConfirmationDTO>("application/json")
                .Produces<UserResponseDTO>()
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity)
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);


            accountRouter.MapPost("/SendEmailConfirmationCode", async (
                EmailConfirmationDTO emailConfirmationDTO,
                UserManager<User> userManager,
                EmailTokenProvider<User> emailTokenProvider,
                IEmailServiceProvider emailServiceProvider) =>
            {
                if (emailConfirmationDTO.ConfirmationCode == null || emailConfirmationDTO.UserId == null)
                {
                    return Results.UnprocessableEntity(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 422,
                        Message = "Unable to send an email confirmation",
                        TimeStamp = DateTime.Now,
                    });
                }

                var user = await userManager.FindByIdAsync(emailConfirmationDTO.UserId);

                if (user == null)
                {
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "User not found",
                        TimeStamp = DateTime.Now,
                    });
                }

                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(emailConfirmationDTO.ConfirmationCode));
                //User can set their own token provider to validate the token. In this case, I am using the default email token provider.
                var result = await emailTokenProvider.ValidateAsync("EmailConfirmation", code, userManager, user);

                if (result)
                {
                    //TODO user needs to replace this url with their app's url
                    await SendEmailConfirmationCode(emailServiceProvider, userEmailAddress: user.Email!, userId: user.Id, code: code);

                    return Results.Ok(new AuthResultDTO
                    {
                        Succeeded = true,
                        StatusCode = 200,
                        Message = "Email successfully sent",
                        TimeStamp = DateTime.Now,
                    });
                }

                return Results.UnprocessableEntity(new AuthResultDTO
                {
                    Succeeded = false,
                    StatusCode = 422,
                    Message = "Unable to send email confirmation code",
                    TimeStamp = DateTime.Now,
                });
            })
                .RequireAuthorization()
                .Accepts<EmailConfirmationDTO>("application/json")
                .Produces<AuthResultDTO>(StatusCodes.Status200OK)
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity)
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);

            accountRouter.MapPost("/updateUserProfile", async (UserProfileDTO userProfileDTO, UserManager<User> userManager) =>
            {
                var user = await userManager.FindByIdAsync(userProfileDTO.UserId);

                if (user == null)
                {
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "User not found",
                        TimeStamp = DateTime.Now,
                    });
                }

                if (userProfileDTO.FirstName != null && userProfileDTO.FirstName != user.FirstName)
                {
                    user.FirstName = userProfileDTO.FirstName;
                }

                if (userProfileDTO.LastName != null && userProfileDTO.LastName != user.LastName)
                {
                    user.LastName = userProfileDTO.LastName;
                }

                if (userProfileDTO.Email != null && userProfileDTO.Email != user.Email)
                {
                    user.Email = userProfileDTO.Email;
                    user.EmailConfirmed = false;

                }

                if (userProfileDTO.PhoneNumber != null && userProfileDTO.PhoneNumber != user.PhoneNumber)
                {
                    user.PhoneNumber = userProfileDTO.PhoneNumber;
                    user.PhoneNumberConfirmed = false;
                }

                await userManager.UpdateAsync(user);

                return Results.Ok(new UserResponseDTO
                {

                    UserId = user.Id,
                    UserName = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    Preferred2fa = user.Preferred2fa,
                    EmailConfirmed = user.EmailConfirmed,

                });

            })
                .RequireAuthorization()
                .Accepts<UserProfileDTO>("application/json")
                .Produces<UserResponseDTO>()
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity)
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);


            accountRouter.MapGet("/userInfo", async (ClaimsPrincipal claimsPrincipal, UserManager<User> userManager) =>
            {
                var userId = claimsPrincipal.Claims.FirstOrDefault(claim => claim.Type == "userId")?.Value;

                if (userId == null)
                {
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "User not found",
                        TimeStamp = DateTime.Now,
                    });
                }

                var user = await userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "User not found",
                        TimeStamp = DateTime.Now,
                    });
                }
                var roles = await userManager.GetRolesAsync(user);
                var claims = await userManager.GetClaimsAsync(user);

                return Results.Ok(new UserDataDTO
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    Preferred2fa = user.Preferred2fa,
                    Roles = roles,
                    Claims = claims
                });


            })
                .RequireAuthorization()
                .Produces<UserDataDTO>()
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);


            accountRouter.MapPost("/sendPhoneConfirmationCode", async (ConfirmPhoneDTO confirmPhoneDTO,
                                                         ISmsServiceProvider smsServiceProvider,
                                                         UserManager<User> userManager) =>
            {
                var user = await userManager.FindByIdAsync(confirmPhoneDTO.UserId);

                if (user == null)
                {
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "User not found",
                        TimeStamp = DateTime.Now,
                    });

                }

                var result = await smsServiceProvider.SendVerificationCodeAsync(user.PhoneNumber!);

                if (result.Status == "Failed")
                {
                    return Results.UnprocessableEntity(new AuthResultDTO
                    {
                        Succeeded = true,
                        StatusCode = 422,
                        Message = "Unable to send the verification through sms",
                        TimeStamp = DateTime.Now,
                    });
                }

                return Results.Ok(new AuthResultDTO
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Message = "We have sent you a verification code",
                    TimeStamp = DateTime.Now,
                });
            })
               .Accepts<ConfirmPhoneDTO>("application/json")
               .Produces<AuthResultDTO>()
               .Produces<AuthResultDTO>(StatusCodes.Status404NotFound)
               .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity);



            accountRouter.MapPost("/confirmPhoneNumber", async (ConfirmPhoneDTO confirmPhoneDTO,
                                                          ISmsServiceProvider smsServiceProvider,
                                                          UserManager<User> userManager) =>
            {
                var user = await userManager.FindByIdAsync(confirmPhoneDTO.UserId);

                if (user == null)
                {
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "User not found",
                        TimeStamp = DateTime.Now,
                    });

                }

                var result = await smsServiceProvider.VerifyPhoneNumberAsync(user.PhoneNumber!, confirmPhoneDTO.VerificationCode!);

                if (result.Status == "approved")
                {
                    user.PhoneNumberConfirmed = true;
                    await userManager.UpdateAsync(user);
                    return Results.Ok(new AuthResultDTO
                    {
                        Succeeded = true,
                        StatusCode = 200,
                        Message = "Phone number successfully verified",
                        TimeStamp = DateTime.Now,
                    });
                }

                return Results.UnprocessableEntity(new AuthResultDTO
                {
                    Succeeded = false,
                    StatusCode = 422,
                    Message = "Unable to verify Phone Number. Try again later",
                    TimeStamp = DateTime.Now,
                });


            })
               .Accepts<ConfirmPhoneDTO>("application/json")
               .Produces<AuthResultDTO>()
               .Produces<AuthResultDTO>(StatusCodes.Status404NotFound)
               .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity); ;

            return app;
        }

        private static async Task SendEmailConfirmationCode(IEmailServiceProvider emailServiceProvider, string userEmailAddress, String userId, string code)
        {
            //Since this an API, should we generate 8 char codes ?
            var callBackUrl = $"http://localhost:3000/confirm-email?confirmationCode={code}&userId={userId}";
            var message = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>.";
            await emailServiceProvider.SendEmailAsync(userEmailAddress, "Confirm your email", message);
        }

        private static string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private static string GenerateQrCodeUri(string email, string unformattedKey, UrlEncoder urlEncoder)
        {
            const string authFormatUri = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
            return string.Format(
                CultureInfo.InvariantCulture,
                authFormatUri,
                urlEncoder.Encode("Microsoft.AspNetCore.Identity.UI"),
                urlEncoder.Encode(email),
                unformattedKey);
        }
    }
}
