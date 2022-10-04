import React, { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../AuthProvider";
import UserAccountMenu from "./UserAccountMenu";
import { Grid } from "@mui/material";

export default function UserAccount() {
  const navigate = useNavigate();
  const { userData, setUserData } = useAuth();

  useEffect(() => {
    const userInfo = async () => {
      await getUserInfo();
    };
    userInfo();
  }, []);

  const getUserInfo = async () => {
    const tokens = sessionStorage.getItem("tokens");
    if (!tokens) {
      navigate("/sign-in");
    }

    let { accessToken } = JSON.parse(tokens);
    await fetchUserInfo(accessToken);
  };

  const fetchUserInfo = async (accessToken) => {
    try {
      let response = await fetch(
        `${process.env.REACT_APP_API_URL}/account/userInfo`,
        {
          headers: {
            Authorization: `Bearer ${accessToken}`,
          },
        }
      );

      if (!response.ok) {
        throw new Error({
          message: "Unauthorized",
          statusCode: response.statusCode,
        });
      }
      var jsonResponse = await response.json();
      setUserData(jsonResponse);
      return response;
    } catch (err) {
      console.log(err);
      if (err.statusCode === 401) {
        //Need to implement a refresh token
        navigate("/sign-in");
      }
      navigate("/forbideen");
    }
  };

  return (
    <Grid container spacing={2}>
      <Grid item xs={12}>
        <UserAccountMenu />
      </Grid>
    </Grid>
  );
}
