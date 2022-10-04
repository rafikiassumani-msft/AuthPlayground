import React, { useState, useReducer, useEffect } from "react";
import {
  Typography,
  Card,
  Grid,
  CardContent,
  Divider,
  Chip,
} from "@mui/material";
import { useAuth } from "../AuthProvider";
import { getData } from "../fetchData";
import VerifyAuthenticatorSetup from "./VerifyAuthenticatorSetup";
import QRCode from "react-qr-code";
import { Stack } from "@mui/system";
import GenerateRecoveryCodes from "./GenerateRecoveryCodes";

const eventFormReducer = (state, eventData) => {
  return {
    ...state,
    [eventData.name]: eventData.value,
  };
};

export default function AuthenticatorData() {
  const [formData, setFormData] = useReducer(eventFormReducer, {});
  const [requestSucceeded, setRequestSucceeded] = useState(false);
  const [authenticatorData, setAuthenticatorData] = useState(null);
  const { userData } = useAuth();

  const handleChange = (event) => {
    setFormData({
      name: event.target.name,
      value: event.target.value,
    });
  };

  const handleSubmit = (event) => {
    event.preventDefault();
  };

  useEffect(() => {
    loadAuthenticatorData();
  }, []);

  const loadAuthenticatorData = async () => {
    try {
      const url = `${process.env.REACT_APP_API_URL}/account/loadAuthenticatorData/${userData.userId}`;
      const tokens = sessionStorage.getItem("tokens");
      const { accessToken } = JSON.parse(tokens);
      const response = await getData({ url, accessToken });

      if (!response.ok) {
        throw new Error("Unable to load authenticator Data");
      }

      var jsonResponse = await response.json();
      if (jsonResponse.preferred2Fa == "Authenticator") {
        console.log(jsonResponse);
        setAuthenticatorData(jsonResponse);
        setRequestSucceeded(true);
      }
    } catch (error) {
      console.log(error);
    }
  };

  return (
    <React.Fragment>
      {requestSucceeded && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12}>
            <Card sx={{ minWidth: 275 }}>
              <CardContent>
                <Typography component="h2" variant="h6">
                  Authenticator data
                </Typography>
                <Divider />
                <Stack sx={{ marginBottom: 2, marginTop: 2 }}>
                  <Chip label={authenticatorData?.authenticatorCode} />
                </Stack>
                <div style={{ height: "auto", maxWidth: 200, width: "100%" }}>
                  <QRCode
                    size={256}
                    style={{ height: "auto", maxWidth: "100%", width: "100%" }}
                    value={authenticatorData?.authenticatorUrl}
                    viewBox={`0 0 256 256`}
                  />
                </div>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12}>
          <Card sx={{ minWidth: 275 }}>
            <CardContent>
              <VerifyAuthenticatorSetup />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <GenerateRecoveryCodes />
    </React.Fragment>
  );
}
