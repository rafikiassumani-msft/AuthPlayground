import React, { useState } from "react";
import {
  Box,
  Card,
  Grid,
  CardContent,
  TextField,
  Button,
  Alert,
} from "@mui/material";

import { useAuth } from "./AuthProvider";
import { postData } from "./fetchData";

const eventFormReducer = (state, eventData) => {
  return {
    ...state,
    [eventData.name]: eventData.value,
  };
};

export default function VerifyPhoneNumber() {
  const [formData, setFormData] = React.useReducer(eventFormReducer, {});
  const [responseSuccess, setResponseSucces] = useState(false);
  const [displayError, setDisplayError] = useState(false);
  const { userData, setUserData } = useAuth();

  const handleChange = (event) => {
    setFormData({
      name: event.target.name,
      value: event.target.value,
    });
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    sendVerificationCode();
  };

  const sendVerificationCode = async () => {
    try {
      const url = `${process.env.REACT_APP_API_URL}/account/confirmPhoneNumber`;
      const tokens = sessionStorage.getItem("tokens");
      const { accessToken } = JSON.parse(tokens);
      const data = { ...formData, userId: userData.userId };
      const response = await postData({ url, accessToken, data });

      if (!response.ok) {
        throw Error("Unable to confirm your phone number");
      }

      setResponseSucces(true);
      setDisplayError(false);
      setUserData({ ...userData, phoneNumberConfirmed: true});
    } catch (error) {
      console.log(error);
      setDisplayError(true);
    }
  };

  return (
    <React.Fragment>
      <Card sx={{ minWidth: 275 }}>
        <CardContent>
          {responseSuccess && (
            <Alert variant="filled" severity="success">
              Phone number successfully verified.
            </Alert>
          )}

          {displayError && (
            <Alert variant="filled" severity="error"> Unable to verify your phone number </Alert>
          )}

          <Box
            component="form"
            noValidate
            onSubmit={handleSubmit}
            sx={{ mt: 3 }}
          >
            <Grid container spacing={2}>
              

              {!userData.phoneNumberConfirmed && (
                <>
                  <Grid item xs={12}>
                    <TextField
                      onChange={handleChange}
                      fullWidth
                      id="verificationCode"
                      label="Verification Code"
                      name="verificationCode"
                    />
                    <Button
                      type="submit"
                      fullWidth
                      variant="contained"
                      sx={{ mt: 3, mb: 2 }}
                    >
                      Verify Phone Number
                    </Button>
                  </Grid>
                </>
              )}
            </Grid>
          </Box>
        </CardContent>
      </Card>
    </React.Fragment>
  );
}
