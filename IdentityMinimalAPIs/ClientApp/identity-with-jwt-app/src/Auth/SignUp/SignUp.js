import * as React from "react";
import Avatar from "@mui/material/Avatar";
import Button from "@mui/material/Button";
import CssBaseline from "@mui/material/CssBaseline";
import TextField from "@mui/material/TextField";
import Link from "@mui/material/Link";
import Grid from "@mui/material/Grid";
import Box from "@mui/material/Box";
import LockOutlinedIcon from "@mui/icons-material/LockOutlined";
import Typography from "@mui/material/Typography";
import Container from "@mui/material/Container";
import { useNavigate } from "react-router-dom";

const eventFormReducer = (state, eventData) => {
  return {
    ...state,
    [eventData.name]: eventData.value,
  };
};

export default function SignUp() {
  const [formData, setFormData] = React.useReducer(eventFormReducer, {});
  const navigate = useNavigate();

  const handleChange = (event) => {
    setFormData({
      name: event.target.name,
      value: event.target.value,
    });
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    SignUp();
  };

  const SignUp = async () => {
    try {
      let response = await fetch(
        `${process.env.REACT_APP_API_URL}/account/signup`,
        {
          method: "post",
          body: JSON.stringify({ ...formData }),
          headers: {
            "content-type": "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error(
          `HTTP Post - Failed to create event - Http status: ${response.status}`
        );
      }

      let jsonResponse = response.json();

      if (!jsonResponse.emailConfirmed) {
        const message = "We just need to verify your email address before you can access your"
          + "account. Please check your mailbox to confirm you recieve a link to verify your email."
        navigate("/notify", {replace: true, state: {alertType: "warning", alertMessage: message}});
      }
    } catch (err) {
      console.log(err);
    }
  };

  return (
    <Container component="main" maxWidth="xs">
      <CssBaseline />
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
          Sign up
        </Typography>
        <Box component="form" noValidate onSubmit={handleSubmit} sx={{ mt: 3 }}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField
                onChange={handleChange}
                autoComplete="given-name"
                name="firstName"
                required
                fullWidth
                id="firstName"
                label="First Name"
                autoFocus
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                onChange={handleChange}
                required
                fullWidth
                id="lastName"
                label="Last Name"
                name="lastName"
                autoComplete="family-name"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                onChange={handleChange}
                required
                fullWidth
                id="email"
                label="Email Address"
                name="email"
                autoComplete="email"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                onChange={handleChange}
                required
                fullWidth
                id="phoneNumber"
                label="You Phone Number"
                name="phoneNumber"
                autoComplete="phoneNumber"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                onChange={handleChange}
                required
                fullWidth
                name="password"
                label="Password"
                type="password"
                id="password"
                autoComplete="new-password"
              />
            </Grid>
          </Grid>
          <Button
            type="submit"
            fullWidth
            variant="contained"
            sx={{ mt: 3, mb: 2 }}
          >
            Sign Up
          </Button>
          <Grid container justifyContent="flex-end">
            <Grid item>
              <Link href="sign-in" variant="body2">
                Already have an account? Sign in
              </Link>
            </Grid>
          </Grid>
        </Box>
      </Box>
    </Container>
  );
}
