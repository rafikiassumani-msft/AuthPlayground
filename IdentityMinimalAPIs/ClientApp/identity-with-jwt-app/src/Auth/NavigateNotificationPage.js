import React from "react";
import { Alert } from "@mui/material";
import { useLocation } from "react-router-dom"

export default function NavigateNotificationPage() {
 
  const { state: { alertType, alertMessage }} = useLocation();

  return (
    <Alert variant="filled" severity={alertType}>
      {alertMessage}
    </Alert>
  );
}
