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
} from "@mui/material";
import { postData } from "../fetchData";
import { useAuth } from "../AuthProvider";

const eventFormReducer = (state, eventData) => {
  return {
    ...state,
    [eventData.name]: eventData.value,
  };
};

export default function ChangePasswordTab() {
  const [formData, setFormData] = React.useReducer(eventFormReducer, {});

  const handleChange = (event) => {
    setFormData({
      name: event.target.name,
      value: event.target.value,
    });
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    changePassword();
  };

  const [requestSucceeded, setRequestSucceeded] = useState(false);
  const [errors, setErrors] = useState([]);
  const { userData } = useAuth();

  const changePassword = async () => {
    try {
      const url = `${process.env.REACT_APP_API_URL}/auth/changePassword`;
      const tokens = sessionStorage.getItem("tokens");
      const { accessToken } = JSON.parse(tokens);
      const changePassObj = {
        userId: userData.userId,
        oldPassword: formData.oldPassword,
        newPassword: formData.newPassword,
      };
      const response = await postData({
        url,
        accessToken,
        data: changePassObj,
      });

      if (!response.ok) {
        if (response.status == 500) {
          throw new Error(
            `Unable to change password - Http code ${response.status}`
          );
        } else {
          throw { ...(await response.json()) };
        }
      }

      setRequestSucceeded(true);
      setErrors([]);
    } catch (error) {
      console.log(error);
      if (error.errors?.length !== undefined) {
        setErrors(error.errors);
      } else {
        setErrors([""].fill("Unable to change password"));
      }
    }
  };

  return (
    <React.Fragment>
      <Typography component="h2" variant="h6">
        Change Your Password
      </Typography>
      <Divider />
      <Card sx={{ minWidth: 275 }}>
        <CardContent>
          {requestSucceeded && (
            <Alert variant="filled" severity="success"> Password successfully changed </Alert>
          )}

          {errors.length > 0 && (
            <Alert severity="error">
              {errors.map((err) => (
                <>
                  {err} <br />
                </>
              ))}
            </Alert>
          )}

          <Box
            component="form"
            noValidate
            onSubmit={handleSubmit}
            sx={{ mt: 3 }}
          >
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  onChange={handleChange}
                  required={true}
                  fullWidth
                  name="oldPassword"
                  label="Current Password"
                  type="password"
                  id="oldPassword"
                  autoComplete="old-password"
                />
              </Grid>

              <Grid item xs={12}>
                <TextField
                  onChange={handleChange}
                  required
                  fullWidth
                  name="newPassword"
                  label="New Password"
                  type="password"
                  id="newPassword"
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
              Change Password
            </Button>
          </Box>
        </CardContent>
      </Card>
    </React.Fragment>
  );
}
