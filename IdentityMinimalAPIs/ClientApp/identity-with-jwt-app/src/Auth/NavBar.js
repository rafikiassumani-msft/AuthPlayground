import React from "react";
import AppBar from "@mui/material/AppBar";
import Toolbar from "@mui/material/Toolbar";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";
import { Link } from "@mui/material";
import { Link as RouterLink, useNavigate } from "react-router-dom";
import { useAuth } from "./AuthProvider";
import { postData } from "./fetchData";

function NavBar() {
  const { isLoggedIn, setIsLoggedIn, userData } = useAuth();
  const navigate = useNavigate();

  const handleLogout = (event) => {
    event.preventDefault();
    logout();
  };

  const logout = async () => {
    const tokens = sessionStorage.getItem("tokens");
    const { accessToken } = JSON.parse(tokens);
    const url = `${process.env.REACT_APP_API_URL}/auth/logout`;

    try {
      const { userId } = userData;
      const data = { userId: userId };
      const response = await postData({ url, accessToken, data });
      if (!response?.ok) {
        throw { ...(await response.json()) };
      }
      sessionStorage.clear();
      setIsLoggedIn(false);
      navigate("/sign-in");
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <AppBar
      position="static"
      color="default"
      elevation={1}
      sx={{ borderBottom: (theme) => `2px solid ${theme.palette.divider}` }}
    >
      <Toolbar sx={{ flexWrap: "wrap" }}>
        <Typography variant="h6" color="inherit" noWrap sx={{ flexGrow: 1 }}>
          ASPNET Core Identity
        </Typography>
        <nav>
          <Link
            component={RouterLink}
            variant="button"
            color="text.primary"
            to="/"
            underline="none"
            sx={{ my: 1, mx: 1.5 }}
          >
            Blogs
          </Link>
          <Link
            component={RouterLink}
            variant="button"
            color="text.primary"
            to="user-account"
            underline="none"
            sx={{ my: 1, mx: 1.5 }}
          >
            My Account
          </Link>
        </nav>
        {isLoggedIn && (
          <Button
            onClick={handleLogout}
            variant="outlined"
            sx={{ my: 1, mx: 1.5 }}
          >
            Logout
          </Button>
        )}

        {!isLoggedIn && (
          <>
            <Button
              component={RouterLink}
              to="sign-in"
              variant="outlined"
              sx={{ my: 1, mx: 1.5 }}
            >
              Login
            </Button>
            <Button
              component={RouterLink}
              to="sign-up"
              variant="outlined"
              sx={{ my: 1, mx: 1.5 }}
            >
              Sign Up
            </Button>
          </>
        )}
      </Toolbar>
    </AppBar>
  );
}

export default NavBar;
