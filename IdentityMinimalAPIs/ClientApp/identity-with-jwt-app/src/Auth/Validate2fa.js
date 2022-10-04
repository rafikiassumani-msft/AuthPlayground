import * as React from "react";
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
import { useAuth } from "./AuthProvider";
import { Alert } from "@mui/material";

export default function Validate2fa() {
  const navigate = useNavigate();
  const { setIsLoggedIn } = useAuth();
  const [displayError, setDisplayError] = React.useState(null);

  const handleSubmit = (event) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    validate2fa(data);
  };

  const validate2fa = async (data) => {
    try {
      let response = await fetch(
        `${process.env.REACT_APP_API_URL}/auth/validate2faCode`,
        {
          method: "post",
          body: JSON.stringify({
            twoFactorCode: data.get("twoFactorCode"),
            email: JSON.parse(sessionStorage.getItem("twoFaRequirements"))
              .email,
          }),
          headers: {
            "content-type": "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error("Unable to validate the auth code");
      }

      let jsonResponse = await response.json();

      if (!jsonResponse.twoFactorAuthSatisfied) {
        navigate("/forbidden");
        //Print error message on the screen
      } else {
        sessionStorage.setItem("tokens", JSON.stringify(jsonResponse));
        setIsLoggedIn(true);
        navigate("/user-account");
      }
    } catch (error) {
      console.error(error);
      setDisplayError(error.message);
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

        <Typography component="h1" variant="h6">
          Verify two factor Code
        </Typography>

        <Typography variant="caption" display="block" gutterBottom>
          (SMS Code, Authenticator Code, Email Code)
        </Typography>

        {displayError && (
          <Alert variant="filled" severity="error">
            {displayError}
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

          {displayError && (
            <Grid container>
              <Grid item xs={12}>
                <Link href="login-with-recovery-code" variant="body2">
                  {"Try a different way? Redeem Auth Code"}
                </Link>
              </Grid>
            </Grid>
          )}
        </Box>
      </Box>
    </Container>
  );
}
