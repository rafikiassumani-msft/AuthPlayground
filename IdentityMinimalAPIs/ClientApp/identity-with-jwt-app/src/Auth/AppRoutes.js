import * as React from "react";
import { Routes, Route } from "react-router-dom";
import SignIn from "./SignIn";
import SignUp from "./SignUp";
import VerifyEmail from "./VerifyEmail";
import Validate2fa from "./Validate2fa";
import UserAccount from "./UserAccount";
import Unauthorized from "./Unauthorized";
import RequireAuth from "./RequireAuth";
import UserAccountMenu from "./UserAccountMenu";
import LoginWithRecoveryCode from "./LoginWithRecoveryCode";
import ForgotPassword from "./ForgotPassword";
import ResetPasword from "./ResetPassword";

function AppRoutes() {
  return (
    <Routes>
      <Route path="/sign-up" element={<SignUp />} />
      <Route path="/sign-in" element={<SignIn />} />
      <Route path="/verify-email" element={<VerifyEmail />} />
      <Route path="/validate-two-fa" element={<Validate2fa />} />
      <Route
        path="/user-account"
        element={
          <RequireAuth>
            <UserAccount />
          </RequireAuth>
        }
      />
      <Route path="/forbideen" element={<Unauthorized />} />
      <Route path="/forgot-password" element={<ForgotPassword />} />
      <Route path="/reset-password" element={<ResetPasword />} />
      <Route path="/login-with-recovery-code" element={<LoginWithRecoveryCode />} />

      <Route path="/menu" element={<UserAccountMenu />} />
    </Routes>
  );
}

export default AppRoutes;
