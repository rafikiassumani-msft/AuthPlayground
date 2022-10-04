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
import { useNavigate } from "react-router-dom";
import { useAuth } from "../AuthProvider";
import { Alert } from "@mui/material";

export default function SignIn() {
  const navigate = useNavigate();
  const { setIsLoggedIn } = useAuth();
  const [logInError, SetLogInError] = useState(false);
  const [loginErrorMessage, SetLogInErrorMessage] = useState(null);

  const handleSubmit = (event) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    signIn(data);
  };

  const signIn = async (data) => {
    try {
      let response = await fetch(
        `${process.env.REACT_APP_API_URL}/auth/login`,
        {
          method: "post",
          body: JSON.stringify({
            email: data.get("email"),
            password: data.get("password"),
          }),
          headers: {
            "content-type": "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error("Unable to login. Your username or password is incorrect. Try again or reset your credentials")
      }

      let jsonResponse = await response.json();

      if (jsonResponse.requiredTwoFactor) {
        sessionStorage.setItem(
          "twoFaRequirements",
          JSON.stringify(jsonResponse)
        );
        navigate("/validate-two-fa");
      } else {
        setIsLoggedIn(true);
        sessionStorage.setItem("tokens", JSON.stringify(jsonResponse));
        navigate("/user-account");
      }
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
          Sign in
        </Typography>

        {logInError && (
          <Alert variant="filled" severity="error">
            {loginErrorMessage}
          </Alert>
        )}
        <Box component="form" onSubmit={handleSubmit} noValidate sx={{ mt: 1 }}>
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
          <TextField
            margin="normal"
            required
            fullWidth
            name="password"
            label="Password"
            type="password"
            id="password"
            autoComplete="current-password"
          />
          <Button
            type="submit"
            fullWidth
            variant="contained"
            sx={{ mt: 3, mb: 2 }}
          >
            Sign In
          </Button>
          <Grid container>
            <Grid item xs>
              <Link href="forgot-password" variant="body2">
                Forgot password?
              </Link>
            </Grid>
            <Grid item>
              <Link href="sign-up" variant="body2">
                {"Don't have an account? Sign Up"}
              </Link>
            </Grid>
          </Grid>
        </Box>
      </Box>
    </Container>
  );
}
