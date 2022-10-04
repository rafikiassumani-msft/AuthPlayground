import * as React from "react";
import CssBaseline from "@mui/material/CssBaseline";
import { ThemeProvider, createTheme } from "@mui/material/styles";
import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import AppRoutes from "./Auth/AppRoutes";
import NavBar from "./Auth/NavBar";
import AuthProvider from "./Auth/AuthProvider";

const theme = createTheme({
  palette: {
    primary: {
      main: "#4615b2",
    },
  },

  typography: {
    fontFamily: ["Open Sans", "sans-serif"].join(","),

    body1: {
      fontWeight: 400,
    },

    body2: {
      fontWeight: 400,
    },

    fontSize: 16,
    button: {
      textTransform: "none",
    },
  },
});

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <AuthProvider>
        <NavBar />
        <Box
          sx={{
            display: "flex",
            flexDirection: "column",
            minHeight: "100vh",
          }}
        >
          <CssBaseline />
          <Container component="main" sx={{ mt: 8, mb: 2 }} maxWidth="lg">
            <AppRoutes />
          </Container>
        </Box>
      </AuthProvider>
    </ThemeProvider>
  );
}
