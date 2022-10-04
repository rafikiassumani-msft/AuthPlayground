import React, { useState } from "react";
import Avatar from "@mui/material/Avatar";
import Button from "@mui/material/Button";
import TextField from "@mui/material/TextField";
import Link from "@mui/material/Link";
import Grid from "@mui/material/Grid";
import Box from "@mui/material/Box";
import LockOutlinedIcon from "@mui/icons-material/LockOutlined";
import Typography from "@mui/material/Typography";
import Container from "@mui/material/Container";
import { Alert } from "@mui/material";

export default function ForgotPassword() {
  const [logInError, SetLogInError] = useState(false);
  const [loginErrorMessage, SetLogInErrorMessage] = useState(null);
  const [requestSucceeded, setRequestSucceeded] = useState(false);

  const handleSubmit = (event) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    forgotPassword(data);
  };

  const forgotPassword = async (data) => {
    try {
      let response = await fetch(
        `${process.env.REACT_APP_API_URL}/auth/forgotPassword`,
        {
          method: "post",
          body: JSON.stringify({
            email: data.get("email"),
          }),
          headers: {
            "content-type": "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error(
          "Unable to reset your password. Invalid username or email address"
        );
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
          Forgot Password
        </Typography>

        {requestSucceeded && (
          <Alert variant="filled" severity="success">
            We sent you an email to help reset your password
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
              id="email"
              label="Email Address"
              name="email"
              autoComplete="email"
              autoFocus
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
            >
              Submit
            </Button>
            <Grid container>
              <Grid item xs>
                <Link href="sign-in" variant="body2">
                  Know your password? Sign In
                </Link>
              </Grid>
            </Grid>
          </Box>
        )}
      </Box>
    </Container>
  );
}
