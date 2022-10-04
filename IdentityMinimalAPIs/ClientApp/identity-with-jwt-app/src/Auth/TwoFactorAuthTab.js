import React, { useReducer, useState } from "react";
import {
  Typography,
  RadioGroup,
  Card,
  Grid,
  CardContent,
  Alert,
  Button,
  Divider,
  Radio,
  FormControlLabel,
  FormGroup,
} from "@mui/material";
import { useAuth } from "./AuthProvider";
import { postData } from "./fetchData";
import AuthenticatorData from "./AuthenticatorData";

const eventFormReducer = (state, eventData) => {
  return {
    ...state,
    [eventData.name]: eventData.value,
  };
};

export default function TwoFactorAuthTab() {
  const { userData, setUserData } = useAuth();

  const [formData, setFormData] = useReducer(eventFormReducer, {});

  const handleChange = (event) => {
    setFormData({
      name: event.target.name,
      value: event.target.value,
    });
  };

  const isEnabled = () => {
    console.log(formData.selected2fa);
    return userData.twoFactorEnabled;
  };

  const isAuthAppRadioButtonEnabled = () => {
    if (
      userData?.preferred2fa === "Phone" ||
      userData?.preferred2fa === "Email"
    ) {
      return true;
    }
    return false;
  };

  const isPhoneRadioButtonEnabled = () => {
    if (
      userData?.preferred2fa === "Authenticator" ||
      userData?.preferred2fa === "Email"
    ) {
      return true;
    }
    return false;
  };

  const isEmailRadioButtonEnabled = () => {
    if (
      userData?.preferred2fa === "Authenticator" ||
      userData?.preferred2fa === "Phone"
    ) {
      return true;
    }
    return false;
  };

  const enableTwoFa = async () => {
    if (formData.selected2fa !== undefined) {
      const url = `${process.env.REACT_APP_API_URL}/account/enable2fa`;
      const jsonResponse = await setTwoFactorAuthFlag(url);
      setUserData({
        ...userData,
        twoFactorEnabled: true,
        preferred2fa: jsonResponse.preferred2fa,
      });
    }
  };

  const disableTwoFa = async () => {
    const url = `${process.env.REACT_APP_API_URL}/account/disable2fa`;
    await setTwoFactorAuthFlag(url);
    setUserData({ ...userData, twoFactorEnabled: false, preferred2fa: null });
  };

  const setTwoFactorAuthFlag = async (url) => {
    try {
      const tokens = sessionStorage.getItem("tokens");
      const { accessToken } = JSON.parse(tokens);
      const data = {
        selected2fa: formData.selected2fa,
        userId: userData.userId,
      };
      const response = await postData({ url, accessToken, data });

      if (!response.ok) {
        throw Error("Unable to set two-fa auth flag");
      }

      return await response.json();
    } catch (error) {
      console.log(error);
    }
  };

  return (
    <React.Fragment>
      <Typography component="h2" variant="h6">
        Enable Two-Factor Auth
      </Typography>
      <Divider />
      <Card sx={{ minWidth: 275, mb: 3 }}>
        <CardContent>
          <Grid container spacing={2}>
            {userData.twoFactorEnabled && (
              <Grid item xs={12}>
                <Alert severity="success">
                  You have two factor authentication enabled
                </Alert>
              </Grid>
            )}

            {!userData.twoFactorEnabled && (
              <Grid item xs={12}>
                <Alert severity="info">
                  You need to activate two-factor authentication
                </Alert>
              </Grid>
            )}
          </Grid>

          <Grid container spacing={2}>
            <Grid item xs={12}>
              <FormGroup>
                <RadioGroup
                  aria-labelledby="choose-two-fa-buttons-group"
                  name="selected2fa"
                  defaultValue={userData?.preferred2fa}
                  key={userData?.preferred2fa}
                  onChange={handleChange}
                >
                  <FormControlLabel
                    disabled={isAuthAppRadioButtonEnabled()}
                    value="Authenticator"
                    control={<Radio />}
                    label="Enable Authenticator App"
                  />
                  {userData.phoneNumberConfirmed && (
                    <FormControlLabel
                      disabled={isPhoneRadioButtonEnabled()}
                      value="Phone"
                      control={<Radio />}
                      label="Enable Two-Factor With SMS"
                    />
                  )}

                  {userData.emailConfirmed && (
                    <FormControlLabel
                      disabled={isEmailRadioButtonEnabled()}
                      value="Email"
                      control={<Radio />}
                      label="Enable Two-Factor With Email"
                    />
                  )}
                </RadioGroup>
              </FormGroup>
            </Grid>
          </Grid>

          <Grid container spacing={2}>
            <Grid item xs={6}>
              <Button
                disabled={isEnabled()}
                onClick={enableTwoFa}
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mt: 3, mb: 2 }}
              >
                Enable Two Factor Auth
              </Button>
            </Grid>

            <Grid item xs={6}>
              <Button
                disabled={!isEnabled()}
                onClick={disableTwoFa}
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mt: 3, mb: 2 }}
              >
                Disable Two Factor Auth
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
      {userData.preferred2fa === "Authenticator" && <AuthenticatorData />}
    </React.Fragment>
  );
}
