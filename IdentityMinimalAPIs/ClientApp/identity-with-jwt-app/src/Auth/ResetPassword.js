import React, { useState } from "react";
import Avatar from "@mui/material/Avatar";
import Button from "@mui/material/Button";
import TextField from "@mui/material/TextField";
import Box from "@mui/material/Box";
import LockOutlinedIcon from "@mui/icons-material/LockOutlined";
import Typography from "@mui/material/Typography";
import Container from "@mui/material/Container";
import { Alert } from "@mui/material";
import { useSearchParams } from "react-router-dom";

export default function ResetPasword() {
  let [searchParams, setSearchParams] = useSearchParams();
  const [logInError, SetLogInError] = useState(false);
  const [loginErrorMessage, SetLogInErrorMessage] = useState(null);
  const [requestSucceeded, setRequestSucceeded] = useState(false);

  const handleSubmit = (event) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    resetPasword(data);
  };

  const resetPasword = async (data) => {
    try {
      let response = await fetch(
        `${process.env.REACT_APP_API_URL}/auth/resetPassword`,
        {
          method: "post",
          body: JSON.stringify({
            userId: searchParams.get("userId"),
            resetPasswordToken: searchParams.get("resetToken"),
            password: data.get("newPassword"),
          }),
          headers: {
            "content-type": "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error("Unable to reset your password- Try again later");
      }

      setRequestSucceeded(true);
    } catch (err) {
      SetLogInError(true);
      SetLogInErrorMessage(err.message);
      console.log(err);
    }
  };

  return (
    <Container component="main" maxWidth="xs">
      <Box
        sx={{
          marginTop: 8,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
        }}
      >
        <Avatar sx={{ m: 1, bgcolor: "secondary.main" }}>
          <LockOutlinedIcon />
        </Avatar>

        <Typography component="h1" variant="h5">
          Reset Your Password
        </Typography>

        {requestSucceeded && (
          <Alert variant="filled" severity="success">
            We have successfully reset your password. Please click on the sign
            in link to log in.
          </Alert>
        )}

        {logInError && (
          <Alert variant="filled" severity="error">
            {loginErrorMessage}
          </Alert>
        )}

        {!requestSucceeded && (
          <Box
            component="form"
            onSubmit={handleSubmit}
            noValidate
            sx={{ mt: 1 }}
          >
            <TextField
              margin="normal"
              required
              fullWidth
              name="newPassword"
              label="New Password"
              type="password"
              id="newPassword"
              autoComplete="new-password"
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
            >
              Submit
            </Button>
          </Box>
        )}
      </Box>
    </Container>
  );
}
