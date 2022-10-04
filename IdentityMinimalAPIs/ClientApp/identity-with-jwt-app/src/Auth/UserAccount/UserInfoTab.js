import React, { useState } from "react";
import {
  Typography,
  Box,
  Card,
  Grid,
  CardContent,
  TextField,
  Button,
  Divider,
  Alert,
  InputAdornment,
} from "@mui/material";

import CheckBoxIcon from "@mui/icons-material/CheckBox";
import { green, red } from "@mui/material/colors";
import { postData } from "../fetchData";
import { useAuth } from "../AuthProvider";

const eventFormReducer = (state, eventData) => {
  return {
    ...state,
    [eventData.name]: eventData.value,
  };
};

export default function UserInfoTab() {
  const [formData, setFormData] = React.useReducer(eventFormReducer, {});
  const [responseSuccess, setResponseSucces] = useState(false);
  const { userData, setUserData } = useAuth();
  const { firstName, lastName, email, phoneNumber, userName } = userData;

  const handleChange = (event) => {
    setFormData({
      name: event.target.name,
      value: event.target.value,
    });
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    updateUserData();
  };

  const updateUserData = async () => {
    try {
      const url = `${process.env.REACT_APP_API_URL}/account/updateUserProfile`;
      const tokens = sessionStorage.getItem("tokens");
      const { accessToken } = JSON.parse(tokens);
      const data = { ...formData, userId: userData.userId };
      console.log(data);
      const response = await postData({ url, accessToken, data });

      if (!response.ok) {
        throw new Error("Unable to update user profile");
      }

      const jsonResponse = await response.json();
      setResponseSucces(true);
      setUserData(jsonResponse);
      console.log(jsonResponse);
    } catch (error) {
      console.log(error);
    }
  };

  return (
    <React.Fragment>
      {responseSuccess && (
        <Alert severity="success">
          You have successfully updated your profile
        </Alert>
      )}
      <Typography component="h2" variant="h6">
        Manage Your Account
      </Typography>
      <Divider />
      <Card sx={{ minWidth: 275 }}>
        <CardContent>
          <Box
            component="form"
            noValidate
            onSubmit={handleSubmit}
            sx={{ mt: 3 }}
          >
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
                  defaultValue={firstName}
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
                  defaultValue={lastName}
                  autoComplete="family-name"
                />
              </Grid>

              <Grid item xs={12}>
                <TextField
                  onChange={handleChange}
                  required
                  fullWidth
                  id="userName"
                  label="Username"
                  defaultValue={userName}
                  name="userName"
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
                  defaultValue={email}
                  autoComplete="email"
                />
              </Grid>

              {!userData.phoneNumberConfirmed && (
                  <Grid item xs={12}>
                    <Typography variant="caption" display="block" sx={{ color: red[800] }} gutterBottom>
                      (Your phone number is not verified and can't be used for two factor auth with sms)
                    </Typography>
                    <TextField
                      onChange={handleChange}
                      fullWidth
                      id="phoneNumber"
                      label="You Phone Number"
                      name="phoneNumber"
                      defaultValue={phoneNumber}
                      autoComplete="phoneNumber"
                      InputProps={{
                        startAdornment: (
                          <InputAdornment position="start">
                            <CheckBoxIcon sx={{ color: red[800] }} />
                          </InputAdornment>
                        ),
                      }}
                    />
                  </Grid>
              )}

              {userData.phoneNumberConfirmed && (
                <Grid item xs={12}>
                  <TextField
                    onChange={handleChange}
                    fullWidth
                    id="phoneNumber"
                    label="You Phone Number"
                    name="phoneNumber"
                    defaultValue={userData.phoneNumber}
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
            </Grid>
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
            >
              Save Changes
            </Button>
          </Box>
        </CardContent>
      </Card>
    </React.Fragment>
  );
}
