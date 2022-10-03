using IdentityMinimalAPIs.DTOs;
using IdentityMinimalAPIs.Models;
using IdentityMinimalAPIs.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SharedIdentity.Abstractions;
using SharedIdentityServices.Abstractions;
using System.Text.Encodings.Web;
using System.Text;

namespace IdentityMinimalAPIs.Services.UserAuthExtensions
{
    public static class AuthExtensions
    {

        public static WebApplication MapUserAuthEndpoints(this WebApplication app)
        {
            var authRouter = app.MapGroup("/auth").WithTags("User Auth Endpoints");

            authRouter.MapPost("/login", async (
                LoginRequestDTO loginRequestDTo,
                SignInManager<User> signInManager,
                UserManager<User> userManager,
                ISmsServiceProvider smsServiceProvider,
                IEmailServiceProvider emailProvider,
                ITokenService tokenService) =>
            {
                var user = await userManager.FindByNameAsync(loginRequestDTo.Email);

                if (user is null)
                {
                    //Don't send a 404 to reveal that the user is not found
                    return Results.UnprocessableEntity(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 422,
                        Message = "Username or password is invalid",
                        TimeStamp = DateTime.Now,
                    });
                }

                //Pre-signing checks
                if (!await signInManager.CanSignInAsync(user))
                {
                    return Results.Forbid();
                }

                if (await userManager.IsLockedOutAsync(user))
                {
                    return Results.Forbid();
                }

                //password check
                if (await userManager.CheckPasswordAsync(user, loginRequestDTo.Password))
                {
                    var alwaysLockout = AppContext.TryGetSwitch("Microsoft.AspNetCore.Identity.CheckPasswordSignInAlwaysResetLockoutOnSuccess", out var enabled) && enabled;
                    var is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user) && (await userManager.GetValidTwoFactorProvidersAsync(user)).Count > 0;

                    if (alwaysLockout || !is2faEnabled)
                    {
                        //reset lockout flag
                        if (userManager.SupportsUserLockout)
                        {
                            await userManager.ResetAccessFailedCountAsync(user);
                        }
                    }
                    //Returns success if two-factor not enabled
                    if (!is2faEnabled)
                    {
                        //Generate access tokens as 2fa is not setup and not required
                        var tokens = await tokenService.GetTokensAsync(user);

                        return Results.Ok(new LoginResponseDTO
                        {
                            TokenType = "Bearer",
                            AccessToken = tokens.AccessToken,
                            RefreshToken = tokens.RefreshToken,
                            RequiredTwoFactor = false,
                        });

                    }


                    //do 2fa auth

                    //need to change this to 1st factor and second factor. 
                    var userPreferred2fa = user.Preferred2fa;

                    if (!string.IsNullOrEmpty(userPreferred2fa))
                    {

                        //If user is using authenticator app, nothing to do. They just need to get the code and validate the code. 

                        if (userPreferred2fa == TwoFactorProviderConstants.PhoneProvider)
                        {
                            if (user.PhoneNumber != null && user.PhoneNumberConfirmed)
                            {
                                var token = await signInManager.UserManager.GenerateTwoFactorTokenAsync(user, userPreferred2fa);
                                await smsServiceProvider.SendSmsAnsync(user.PhoneNumber, token);
                            }
                        }

                        if (userPreferred2fa == TwoFactorProviderConstants.EmailProvider)
                        {
                            if (user.EmailConfirmed)
                            {
                                var token = await signInManager.UserManager.GenerateTwoFactorTokenAsync(user, userPreferred2fa);
                                await emailProvider.Send2faCodeEmailAsync(user.NormalizedEmail!, token);
                            }
                        }

                    }

                    return Results.Ok(new LoginResponseDTO
                    {
                        RequiredTwoFactor = true,
                        TwoFactorAuthSatisfied = false,
                        EnabledMfas = new List<string> { userPreferred2fa! },
                        Email = user.Email!
                    });
                }


                return Results.Unauthorized();
            })
                .Accepts<LoginRequestDTO>("application/json")
                .Produces<LoginResponseDTO>()
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity);

            authRouter.MapPost("/validate2faCode", async(
                HttpContext context,
                TwoFactorAuthRequestDTO twoFactorAuthDTO,
                SignInManager<User> signInManager,
                UserManager<User> userManager,
                ITokenService tokenService) =>
            {
                var authCode = twoFactorAuthDTO.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

                var user = await userManager.FindByEmailAsync(twoFactorAuthDTO.Email);

                if (user == null)
                {
                    return Results.NotFound(new { Message = " User not found" });
                }

                var isValid = await userManager.VerifyTwoFactorTokenAsync(user, tokenProvider: user.Preferred2fa!, authCode);

                if (isValid)
                {
                    //Generate tokens
                    var tokenDTO = await tokenService.GetTokensAsync(user);

                    return Results.Ok(new LoginResponseDTO
                    {
                        TokenType = "Bearer",
                        AccessToken = tokenDTO.AccessToken,
                        RefreshToken = tokenDTO.RefreshToken,
                        TwoFactorAuthSatisfied = isValid,
                    });
                }

                //Experimenting with problemDetails with RequestId
                return Results.UnprocessableEntity(new AuthResultProblemDTO
                {
                    Succeeded = false,
                    Detail = "Invalid token",
                    Title = "Unable to validate the token you provided",
                    RequestId = context.TraceIdentifier,
                    Instance = context.Request.Path,
                    TimeStamp = DateTime.Now,
                });

            })
                .Accepts<TwoFactorAuthRequestDTO>("application/json")
                .Produces<LoginResponseDTO>()
                .Produces<AuthResultProblemDTO>(StatusCodes.Status403Forbidden);


            authRouter.MapPost("/logout", async (LogoutRequestDTO logoutDTO, HttpRequest httpRequest, IJwtRevocationStrategyFactory factory) =>
            {

                var authHeader = httpRequest.Headers["Authorization"].FirstOrDefault()?.Split(" ");
                var accessToken = authHeader?.Last();

                if (accessToken == null)
                {
                    return Results.UnprocessableEntity(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 422,
                        Message = "Invalid Access Token supplied",
                        TimeStamp = DateTime.Now,
                    });
                }

                var revocationService = factory.CreateStrategy();

                var result = await revocationService.RevokeAsync(jwtToken: accessToken, userId: logoutDTO.UserId);

                if (!result.Succeeded)
                {
                    return Results.UnprocessableEntity(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 422,
                        Message = "Unable to log out the user",
                        TimeStamp = DateTime.Now,
                    });

                }

                return Results.Ok(new AuthResultDTO
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Message = "User successfully logged out",
                    TimeStamp = DateTime.Now,
                });
            })
                .RequireAuthorization()
                .Accepts<LogoutRequestDTO>("application/json")
                .Produces<AuthResultDTO>()
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity);


            authRouter.MapPost("/revoke", async (
                JwtRevocationRequestDTO revocationDTO,
                IJwtRevocationStrategyFactory factory) =>
            {

                var revocationService = factory.CreateStrategy();

                var result = await revocationService.RevokeAsync(jwtToken: revocationDTO.JwtToken, userId: revocationDTO.UserId);

                if (result.Succeeded)
                {
                    return Results.Ok(new AuthResultDTO
                    {
                        Succeeded = true,
                        StatusCode = 200,
                        Message = "Token successfully revoked",
                        TimeStamp = DateTime.Now,
                    });

                }

                return Results.UnprocessableEntity(new AuthResultDTO
                {
                    Succeeded = false,
                    StatusCode = 422,
                    Message = "Unable to revoke the access token",
                    TimeStamp = DateTime.Now,
                });

            })
                .RequireAuthorization()
                .Accepts<JwtRevocationRequestDTO>("application/json")
                .Produces<AuthResultDTO>()
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity);

            authRouter.MapPost("/refreshToken", async (RefreshTokenDTO refreshTokenDTO, ITokenService tokenService, UserManager<User> userManager) =>
            {
                var isValid = await tokenService.ValidateRefreshToken(refreshTokenDTO.RefreshToken);

                if (isValid)
                {
                    var user = await userManager.FindByIdAsync(refreshTokenDTO.UserId);

                    if (user == null)
                    {
                        return Results.NotFound(new AuthResultDTO
                        {
                            Succeeded = false,
                            StatusCode = 404,
                            Message = "Could find the principal of the provided refresh token",
                            TimeStamp = DateTime.Now,
                        });
                    }

                    var tokens = await tokenService.GetTokensAsync(user);
                    //Revoke the current refresh token by changing the expiry time and setting the isRevoked flag to true. 
                    await tokenService.RevokeRefreshTokenAsync(user, refreshTokenDTO.RefreshToken);
                    return Results.Ok(tokens);
                }

                return Results.UnprocessableEntity(new { Message = "Could not refresh the tokens" });
            })
                .RequireAuthorization()
                .Accepts<RefreshTokenDTO>("application/json")
                .Produces<TokenDTO>()
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);


            authRouter.MapPost("/changePassword", async (ChangePasswordDTO changePasswordDTO, UserManager<User> userManager) =>
            {

                var user = await userManager.FindByIdAsync(changePasswordDTO.UserId);
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

                var changePassResult = await userManager.ChangePasswordAsync(user,
                    currentPassword: changePasswordDTO.OldPassword,
                    newPassword: changePasswordDTO.NewPassword);

                if (!changePassResult.Succeeded)
                {
                    return Results.UnprocessableEntity(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 422,
                        Message = "Unable to change password",
                        Errors = changePassResult.Errors.Select(e => e.Description).ToList(),
                        TimeStamp = DateTime.Now,
                    }); ;
                }
                //TOD invalidate access token ?

                return Results.Ok(new AuthResultDTO
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Message = "Successfully changed password",
                    TimeStamp = DateTime.Now,
                });
            })
                .RequireAuthorization()
                .Accepts<ChangePasswordDTO>("application/json")
                .Produces<AuthResultDTO>()
                .Produces<AuthResultDTO>(StatusCodes.Status422UnprocessableEntity)
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);

            authRouter.MapPost("/forgotPassword", async (ForgotPasswordDTO forgotPasswordDTO, UserManager<User> userManager, IEmailServiceProvider emailService) =>
            {
                var user = await userManager.FindByEmailAsync(forgotPasswordDTO.Email);

                if (user == null)
                {
                    //TODO, might need to return 422 with generic error
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "A user with this name/email could not be found",
                        TimeStamp = DateTime.Now,
                    });
                }

                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                //TODO use environment variables for email reset callback
                var callbackUrl = $"http://localhost:3000/reset-password?resetToken={code}&userId={user.Id}";

                await emailService.SendEmailAsync(
                    user.Email!,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return Results.Ok(new
                {
                    EmailSent = true,
                    StatusCode = 200,
                    PasswordResetToken = code,
                    Message = "We sent you an email to help reset your password",
                    TimeStamp = DateTime.Now,
                });
            })
                .Accepts<ForgotPasswordDTO>("application/json")
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);

            authRouter.MapPost("/resetPassword", async (ResetPasswordDTO resetPasswordDTO, UserManager<User> userManager) =>
            {
                var user = await userManager.FindByIdAsync(resetPasswordDTO.UserId);

                if (user == null)
                {
                    //TODO, might need to return 422 with generic error
                    return Results.NotFound(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Message = "A user with this name/email could not be found",
                        TimeStamp = DateTime.Now,
                    });
                }

                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordDTO.ResetPasswordToken));
                var result = await userManager.ResetPasswordAsync(user, code, resetPasswordDTO.Password);

                if (!result.Succeeded)
                {
                    return Results.UnprocessableEntity(new
                    {
                        Succeeded = false,
                        StatusCode = 422,
                        Message = "Unable to change reset password",
                        Errors = result.Errors.Select(err => err.Description).ToList(),
                        TimeStamp = DateTime.Now,
                    });

                }

                return Results.Ok(new
                {
                    EmailSent = true,
                    StatusCode = 200,
                    PasswordResetToken = code,
                    Message = "Your password was successfully reset",
                    TimeStamp = DateTime.Now,
                });

            })
                .Accepts<ForgotPasswordDTO>("application/json")
                .Produces<AuthResultDTO>(StatusCodes.Status404NotFound);


            authRouter.MapPost("/loginWithRecoveryCode", async (
                                            LoginWithRecoveryCodeDTO loginWithRecoveryCodeDTO,
                                            SignInManager<User> signInManager,
                                            UserManager<User> userManager,
                                            ITokenService tokenService) =>
            {
                var user = await userManager.FindByEmailAsync(loginWithRecoveryCodeDTO.Email);

                if (user is null)
                {
                    return Results.UnprocessableEntity(new AuthResultDTO
                    {
                        Succeeded = false,
                        StatusCode = 422,
                        Message = "Unable to login with the recovery code you provided",
                        TimeStamp = DateTime.Now,
                    });
                }

                var recoveryCode = loginWithRecoveryCodeDTO.RecoveryCode.Replace(" ", string.Empty);

                var result = await userManager.RedeemTwoFactorRecoveryCodeAsync(user, recoveryCode);

                if (result.Succeeded)
                {
                    var tokens = await tokenService.GetTokensAsync(user);

                    return Results.Ok(new LoginResponseDTO
                    {
                        TokenType = "Bearer",
                        AccessToken = tokens.AccessToken,
                        RefreshToken = tokens.RefreshToken,
                        TwoFactorAuthSatisfied = true,
                    });
                }


                return Results.Json(new AuthResultDTO
                {
                    Succeeded = false,
                    StatusCode = 401,
                    Message = "Unable to sign in with the provided recovery code",
                    TimeStamp = DateTime.Now,
                },
                statusCode: StatusCodes.Status401Unauthorized);
            })
                .Accepts<LoginWithRecoveryCodeDTO>("application/json")
                .Produces<LoginResponseDTO>()
                .Produces<AuthResultDTO>(StatusCodes.Status401Unauthorized);


            return app;
        }
    }
}
