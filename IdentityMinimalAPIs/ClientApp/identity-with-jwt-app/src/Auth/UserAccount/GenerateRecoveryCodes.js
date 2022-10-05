import React, { useState, useEffect } from "react";
import {
  Typography,
  Card,
  Grid,
  CardContent,
  Alert,
  Divider,
  Chip,
  Stack,
} from "@mui/material";
import LoadingButton from "@mui/lab/LoadingButton";
import SaveIcon from "@mui/icons-material/Save";
import { useAuth } from "../AuthProvider";
import { getData } from "../fetchData";

export default function GenerateRecoveryCodes() {
  const [displayError, setDisplayError] = useState(false);
  const [authRecoveryCodes, setAuthRecoveryCodes] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const { userData } = useAuth();

  const generateRecoveryCodes = async () => {
    try {
      setIsLoading(true);
      const url = `${process.env.REACT_APP_API_URL}/account/recoveryCodes/${userData.userId}`;
      const tokens = sessionStorage.getItem("tokens");
      const { accessToken } = JSON.parse(tokens);
      const response = await getData({ url, accessToken });

      if (!response.ok) {
        throw new Error("Unable to generate recovery codes");
      }

      const { recoveryCodes } = await response.json();
      setAuthRecoveryCodes(recoveryCodes);
      setIsLoading(false);
    } catch (error) {
      console.log(error);
      setDisplayError(error.message);
      setIsLoading(false);
    }
  };

  return (
    <React.Fragment>
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12}>
          <Card sx={{ minWidth: 275 }}>
            <CardContent>
              <Typography component="h2" variant="h6">
                Generate recovery codes
              </Typography>
              <Divider />
              <Typography variant="caption" display="block" gutterBottom>
                The following action will generate and replace your current
                recovery codes. Please copy the codes in a safe place
              </Typography>
              {displayError && (
                <Alert variant="filled" severity="error">
                  Unable to generate recovery codes
                </Alert>
              )}
              <LoadingButton
                fullWidth
                sx={{ marginBottom: 2, marginTop: 2 }}
                onClick={generateRecoveryCodes}
                loading={isLoading}
                loadingPosition="start"
                startIcon={<SaveIcon />}
                variant="outlined"
              >
                Generate/Reset recovery codes
              </LoadingButton>
            </CardContent>
          </Card>
        </Grid>

        {authRecoveryCodes.length !== 0 && (
          <Grid item xs={12}>
            <Card sx={{ minWidth: 275 }}>
              <CardContent>
                <Typography component="h2" variant="h6">
                  Recovery codes
                </Typography>
                <Divider />
                <Stack spacing={1} sx={{ marginBottom: 2, marginTop: 2 }}>
                  {authRecoveryCodes.map((code) => (
                    <Chip label={code} />
                  ))}
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>
    </React.Fragment>
  );
}
