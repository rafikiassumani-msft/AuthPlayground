import * as React from "react";
import Avatar from "@mui/material/Avatar";
import Button from "@mui/material/Button";
import TextField from "@mui/material/TextField";
import Box from "@mui/material/Box";
import LockOutlinedIcon from "@mui/icons-material/LockOutlined";
import Typography from "@mui/material/Typography";
import Container from "@mui/material/Container";
import { useNavigate } from "react-router-dom";
import { useAuth } from "./AuthProvider";
import { Alert } from "@mui/material";

export default function LoginWithRecoveryCode() {
  const navigate = useNavigate();
  const { setIsLoggedIn } = useAuth();
  const [displayError, setDisplayError] = React.useState();

  const handleSubmit = (event) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    redeemRecoveryCode(data);
  };

  const redeemRecoveryCode = async (data) => {
    try {
      let response = await fetch(
        `${process.env.REACT_APP_API_URL}/auth/loginWithRecoveryCode`,
        {
          method: "post",
          body: JSON.stringify({
            recoveryCode: data.get("recoveryCode"),
            email: JSON.parse(sessionStorage.getItem("twoFaRequirements")).email,
          }),
          headers: {
            "content-type": "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error("Unable to login with the recovery code that you provided");
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
          Login with a recovery code
        </Typography>

        <Typography variant="caption" display="block" gutterBottom>
          (Please use the generated recovery codes to login)
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
            id="recoveryCode"
            label="Recovery Code"
            name="recoveryCode"
            autoFocus
          />
          <Button
            type="submit"
            fullWidth
            variant="contained"
            sx={{ mt: 3, mb: 2 }}
          >
            Login With Recovery Code
          </Button>
        </Box>
      </Box>
    </Container>
  );
}
