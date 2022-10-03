using Microsoft.EntityFrameworkCore;
using IdentityMinimalAPIs.Models;
using IdentityMinimalAPIs.Data;
using Microsoft.AspNetCore.Identity;
using IdentityMinimalAPIs.Services.ConfigExtensions;
using SharedIdentity.Models;
using SharedIdentity.Abstractions;
using SharedIdentity.Services;
using IdentityMinimalAPIs.Services.Auth;
using IdentityMinimalAPIs.Services.UserAuthExtensions;
using SharedIdentityServices.Abstractions;
using IdentityMinimalAPIs.DTOs;
using IdentityMinimalAPIs.Services.TokenServices;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("IdentityMinimalAPIsContextConnection") ?? throw new InvalidOperationException("Connection string 'IdentityMinimalAPIsContextConnection' not found.");

builder.Services.AddDbContext<IdentityMinApiDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentityCore<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddSignInManager<SignInManager<User>>()
    .AddEntityFrameworkStores<IdentityMinApiDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication("CustomJwt").AddScheme<CustomJwtAuthenticationOptions, CustomJwtAuthenticationHandler>("CustomJwt", null);

builder.Services.AddJwtRevocationStrategy(options =>
{
    options.StrategyName = JwtRevocationStrategyConstants.JtiMatchter;
});

builder.Services.AddAuthorization();
builder.Services.AddCors();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.Configure<TwilioSettings>(builder.Configuration.GetRequiredSection("Twilio"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetRequiredSection("JwtSettings"));
builder.Services.AddScoped<ITokenService, JwtTokenGeneratorService>();
builder.Services.AddSingleton<IEmailServiceProvider, SendGridEmailService>();
builder.Services.AddSingleton<ISmsServiceProvider, TwiloSmsService>();
builder.Services.AddSingleton<JwtSecurityTokenHandlerFactory>();
builder.Services.AddHttpLogging( loggingOpts =>
{
    loggingOpts.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    loggingOpts.MediaTypeOptions.AddText("application/javascript");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseHttpLogging();

app.UseCors(policy  =>  {
    policy.AllowCredentials().AllowAnyHeader().WithOrigins("http://localhost:3000");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => $"app is runing {DateTime.Now}");

app.MapUserRegistrationEndpoints();
app.MapUserAuthEndpoints();


app.Run();
