import React, { useState } from "react";
import {
  Typography,
  Card,
  Grid,
  CardContent,
  TextField,
  Button,
  Divider,
  InputAdornment,
  Alert,
} from "@mui/material";
import { green, red } from "@mui/material/colors";

import CheckBoxIcon from "@mui/icons-material/CheckBox";
import CancelIcon from "@mui/icons-material/Cancel";
import { useAuth } from "../AuthProvider";
import { postData } from "../fetchData";
import VerifyPhoneNumber from "./VerifyPhoneNumber";

export default function PhoneNumberSettingsTab() {
  const [responseSuccess, setResponseSucces] = useState(false);
  const [displayError, setDisplayError] = useState(false);
  const [errorMessage, setErrorMessage] = useState(null);
  const { userData, setUserData } = useAuth();

  const handleSubmit = (event) => {
    event.preventDefault();
    sendVerificationCode();
  };

  const sendVerificationCode = async () => {
    try {
      const url = `${process.env.REACT_APP_API_URL}/account/sendPhoneConfirmationCode`;
      const tokens = sessionStorage.getItem("tokens");
      const { accessToken } = JSON.parse(tokens);
      const data = { userId: userData.userId };
      const response = await postData({ url, accessToken, data });

      if (!response.ok) {
        if (response.status == 422) {
          var { message } = await response.json();
          throw new Error(message);
        } else {
          throw new Error(
            "Unable to send an sms verification to your phone number"
          );
        }
      }

      setResponseSucces(true);
      setUserData({ ...userData});
    } catch (error) {
      setDisplayError(true);
      setErrorMessage(error.message);
      console.log(error);
    }
  };

  return (
    <React.Fragment>
      <Typography component="h2" variant="h6">
        Verify Your Phone Number
      </Typography>
      <Divider />
      <Card sx={{ minWidth: 275 }}>
        <CardContent>
          {responseSuccess && (
            <Alert variant="filled" severity="success">
              We have sent you a verification code through sms.
            </Alert>
          )}

          {displayError && (
            <Alert variant="filled" severity="error">
              {errorMessage}
            </Alert>
          )}

          <Grid container spacing={2}>
            {userData.phoneNumberConfirmed && (
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  disabled
                  id="phoneNumber"
                  label="You Phone Number"
                  name="phoneNumber"
                  value={userData.phoneNumber}
                  autoComplete="phoneNumber"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <CheckBoxIcon sx={{ color: green[800] }} />
                      </InputAdornment>
                    ),
                  }}
                />
              </Grid>
            )}

            {!userData.phoneNumberConfirmed && !responseSuccess && (
              <>
                <Grid item xs={12}>
                  <Alert severity="error" sx={{ mb: 3 }}>
                    Your phone number is not verified
                  </Alert>
                  <TextField
                    fullWidth
                    disabled
                    id="phoneNumber"
                    label="You Phone Number"
                    name="phoneNumber"
                    value={userData.phoneNumber}
                    autoComplete="phoneNumber"
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="end">
                          <CancelIcon sx={{ color: red[800] }}/>
                        </InputAdornment>
                      ),
                    }}
                  />
                  <Button
                    onClick={handleSubmit}
                    type="submit"
                    fullWidth
                    variant="contained"
                    sx={{ mt: 3, mb: 2 }}
                  >
                    Send verification sms code
                  </Button>
                </Grid>
              </>
            )}
          </Grid>
        </CardContent>
      </Card>
      {responseSuccess && <VerifyPhoneNumber />}
    </React.Fragment>
  );
}
