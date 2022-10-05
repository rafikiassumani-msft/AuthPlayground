# ASPNET Core Identity with Minimal APIs & Reactjs UI

This project create aspnet core Identity http endpoints with Minimal API features and a [Reactjs](https://github.com/rafikiassumani-msft/AuthPlayground/tree/main/IdentityMinimalAPIs/ClientApp/identity-with-jwt-app) app that implements the ASPNET Identity UI screens. It uses `IUserManager`, `ISigningManager` and other existing ASPNET Core Identity services.  The project implements a two factor auth using email, sms (Twilio) and TOTP Authenticator app. Upon signing up and email confirmation, the user can choose to set-up one of the provided two-factor (Email, Phone, Authenticator App) methods for two-factor auth. Below is the list of provided http endpoints:
![image](https://user-images.githubusercontent.com/87031580/194159455-3c215c4c-afdf-49de-9333-26a6b3e14bb1.png)
![image](https://user-images.githubusercontent.com/87031580/194159617-e3d95945-9b4f-462b-b5b6-bdc58f6f90f1.png)

## Custom JWT integration

When the user successfully authenticate (After two-factor), the auth endpoint generates and returns two tokens: 
1. Access token: This is a custom JWT token with user data as claims that can be used for consuming other protected endpoints
2. Refresh token: This is an opaque UUID token that gets generated and stored in the DB. the value of this token is the same as the JTI value for access token. It can be used to refresh the access token, though the refresh endpoint. 

## JWT Token Invalidations

This project uses three strategies for JWT token (Access token) invalidation. These three strategies rely on the DB queries, therefore making the jwt scenarios stateful. You can configure your preferred strategy using below code:

```C#

builder.Services.AddJwtRevocationStrategy(options =>
{
    options.StrategyName = JwtRevocationStrategyConstants.JtiMatchter;
    //options.StrategyName = JwtRevocationStrategyConstants.AllowList;
    //options.StrategyName = JwtRevocationStrategyConstants.Denylist;
});

```

In case you don't want to bother about token invalidation, you can just configure your JWT tokens to be short lived (10-15 min) and use the refresh token endpoint to re-issue the access token and keep the user logged. You'll need, however have to change the authentication configuration to use the jwtbearer validation mechanisms that come with ASPNET Core. 

### 1. JTI Matcher Strategy

We store the access token's jti (unique identifier) with the user info. This is an additional column in the user info table. Upon a successful authentication and token generation, this jti value is saved to the user table. When the user calls the `/auth/logout` endpoint, we set the jti column to `null`. To validate if a token has not been revoked, the custom auth middleware queries the DB database for the jti value and compares it with the provided token jti value. if the values match, the calls goes through, otherwise the call is rejected with 401 http status code.

### 2. Allowed List Strategy

With this strategt, we create an allow list table that stores the userId, information from the token (jti) and token experition timestamp. Rather than storing the full blown JWT, we store the jti instead. When the user successfully authenticates, we generate a jwt token and stores its jti in the allowed list table. To validate if the token has not been revoked, the custom middleware decodes the token, gets its jti matcher and queries the allowed list table and compares the token jti values and the experation timestamp. The user can setup a batch process for cleaning up this table based on token experition timestamp. 

### 3. Deny(Disalow) List Strategy

When the token is revoked either through calling `/auth/logout` or `auth/revoke` endpoints, the token's jti, user Id and token experition timestamp are stored in a disallowed table. When the user calls a protected endpoint, the custom middleware decodes the token, queries the database for the jti value and compares the decoded jti claim to the value from the DB. If they match, then we know the token has been revoked and therefore the middleware rejects the request with 401 http status code.

## How to run the apps

### 1. Backend

1. The backend relies on ef core. You'll need to generate the ef core schemas `dotnet ef migrations add InitialIdenityMigration` and then ` ef dotnet ef database update ` to create your DB and tables. 
2. For email confirmation, phone number confirmation and two factor auth to work, you'll need to provide the following secrets: 

```JSON
{
  "Twilio:TwilioPhoneNumber": "Your twilio number",
  "Twilio:AuthToken": "Your twilio auth token",
  "Twilio:AccountSID": "Your twilio account sid",
  "SendGridEmail:ApiKey": "your sendgrip API key",
  "Twilio:VerificationServiceSID": "Your twilio verification sid for phone number verifications",
  "JwtSettings:Issuer": "https://localhost:7115 - Can be replaced with your own",
  "JwtSettings:Audience": "https://localhost:7115 - Can be replaced with your own",
  "JwtSettings:TokenSecretKey": "You jwt token secrets used for signing the tokens"
}

```

### 2. Frontend

1. Ensure you have the latest nodejs and run the following command: 

`npm install`
`npm start`

Your app should be served at port 3000. Depending on the port for your backend, you may need to change the following value of the url in the `.env.development` 

`REACT_APP_API_URL=https://localhost:7115`

## Some UI Screens

### 1. Sign up 

<img width="362" alt="image" src="https://user-images.githubusercontent.com/87031580/194180123-a46556f5-b2c0-4570-aba6-7a5e8946ffca.png">

### 2. Sign In flow with Authenticator
![sign-in-image-flow](https://user-images.githubusercontent.com/87031580/194182152-02d302db-428f-4d9b-9608-27a786735819.png)

The user can also log in by redeeming one of the authenticator recovery codes.

### 3. Other available screens (Forgot Passowrd flow, Login With Authenticator Code, Validate Email, Confirm Phone Number)

## TODOS
 1. Implement the cookie auth for Single page apps hosted on the same domain as the backend and server side rendered SPAs. 
 2. Deploy these apps to Azure with terraform.
 3. Write tests for the Minimal API backend and reactjs frontend.
