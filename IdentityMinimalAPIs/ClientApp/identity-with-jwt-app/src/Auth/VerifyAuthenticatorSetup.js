import React, { useState } from "react";
import Button from "@mui/material/Button";
import TextField from "@mui/material/TextField";
import { Alert, Divider } from "@mui/material";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { useAuth } from "./AuthProvider";

export default function VerifyAuthenticatorSetup() {
  const { userData } = useAuth();
  const [isAppVerified, setIsAppVerified] = useState(false);
  const [displayError, setDisplayError] = useState(false);

  const handleSubmit = (event) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    verify2faApp(data);
  };

  const verify2faApp = async (data) => {
    try {
      const tokens = sessionStorage.getItem("tokens");
      const { accessToken } = JSON.parse(tokens);
      let response = await fetch(
        `${process.env.REACT_APP_API_URL}/account/verifyAuthenticatorApp`,
        {
          method: "post",
          body: JSON.stringify({
            twoFactorCode: data.get("twoFactorCode"),
            email: userData.email,
          }),
          headers: {
            "content-type": "application/json",
            Accept: "application/json",
            Authorization: `Bearer ${accessToken}`,
          },
        }
      );

      if (!response.ok) {
        throw new Error(
          `HTTP Post - Fail to validate the auth code in: ${response.status}`
        );
      }

      setIsAppVerified(true);
    } catch (error) {
      console.error(error);
      setDisplayError(true);
    }
  };

  return (
    <>
      <Typography component="h1" variant="h6">
        Verify your authenticor app
      </Typography>
      <Divider />
      <Typography variant="caption" display="block" gutterBottom>
        (Please open your app and enter auth code to verify the setup)
      </Typography>
      {isAppVerified && (
        <Alert variant="filled" severity="success">
          You have correctly setup the authenticator app
        </Alert>
      )}
      {displayError && (
        <Alert variant="filled" severity="error">
          Unable to verify your authenticator app
        </Alert>
      )}
      <Box component="form" onSubmit={handleSubmit} noValidate sx={{ mt: 1 }}>
        <TextField
          margin="normal"
          required
          fullWidth
          id="twoFactorCode"
          label="Two factor Code"
          name="twoFactorCode"
          autoFocus
        />
        <Button
          type="submit"
          fullWidth
          variant="contained"
          sx={{ mt: 3, mb: 2 }}
        >
          Verify Code
        </Button>
      </Box>
    </>
  );
}
