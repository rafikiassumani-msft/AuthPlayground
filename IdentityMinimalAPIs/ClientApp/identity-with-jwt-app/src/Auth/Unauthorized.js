import React from "react";
import { Alert } from "@mui/material";

export default function Unauthorized() {
  return (
    <Alert variant="filled" severity="error">
      You are not authorized to view the resource
    </Alert>
  );
}
