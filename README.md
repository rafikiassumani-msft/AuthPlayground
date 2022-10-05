# ASPNET Core Identity with Minimal APIs

This project create aspnet core Identity http endpoints with Minimal API features. It uses `IUserManager`, `ISigningManager` and other existing ASPNET Core Identity services.  The project implements a two factor auth using email, sms (Twilio) and TOTP Authenticator app. Upon signing up and email confirmation, the user can choose to set-up one of the provided two-fa methods (Email, Phone, Authenticator App) methods for two-factor auth. Below is the list of provided http endpoints:
![image](https://user-images.githubusercontent.com/87031580/194159455-3c215c4c-afdf-49de-9333-26a6b3e14bb1.png)
![image](https://user-images.githubusercontent.com/87031580/194159617-e3d95945-9b4f-462b-b5b6-bdc58f6f90f1.png)

## Custom JWT integration

When the user successfully authenticate (After two-factor), the auth endpoint generates and returns two tokens: 
1. Access token: This is a custom JWT token with user data as claims that can be used for consuming other protected endpoints
2. Refresh token: This is an opaque UUID token that gets generated and stored in the DB. the value of this token is the same as the JTI value for access token. It can be used to refresh the access token, though the refresh endpoint. 

## JWT Token Invalidations

This project uses three strategies for JWT token (Access token) invalidation. 

### JTI Matcher Strategy

We store the access token jti (unique identifier) with the user info. This is an additional column in the user info table. Upon successfully authentication and toke generation, this jti value is saved to the user table. When the user calls the `/logout` endpoint, we set the jti column to null. To validate if a token has not been revoked, our custom auth middleware query the DB database for the jti value and compares it with the provided token jti value. if they match, the calls goes through.

