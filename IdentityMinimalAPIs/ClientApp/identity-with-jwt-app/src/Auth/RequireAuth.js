import React from "react";
import { useAuth } from "./AuthProvider";
import { Navigate } from "react-router-dom";

const RequireAuth = ({ children }) => {
  const { isLoggedIn } = useAuth();

  if (isLoggedIn) {
    return children;
  } else {
    return <Navigate to="/sign-in" />;
  }
};

export default RequireAuth;
