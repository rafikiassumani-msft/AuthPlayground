import * as React from "react";
import { Routes, Route } from "react-router-dom";
import SignIn from "./SignIn/SignIn";
import SignUp from "./SignUp/SignUp";
import VerifyEmail from "./UserAccount/VerifyEmail";
import Validate2fa from "./SignIn/Validate2fa";
import UserAccount from "./UserAccount/UserAccount";
import Unauthorized from "./Unauthorized";
import RequireAuth from "./RequireAuth";
import UserAccountMenu from "./UserAccount/UserAccountMenu";
import LoginWithRecoveryCode from "./SignIn/LoginWithRecoveryCode";
import ForgotPassword from "./SignIn/ForgotPassword";
import ResetPasword from "./SignIn/ResetPassword";
import NavigateNotificationPage from "./NavigateNotificationPage";

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
      <Route path="/notify" element={<NavigateNotificationPage />} />
    </Routes>
  );
}

export default AppRoutes;
