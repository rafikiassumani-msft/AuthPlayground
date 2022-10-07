import React, { useState, useEffect } from "react";
import { useParams, useSearchParams} from "react-router-dom";
import { Alert, Link } from "@mui/material";

const VerifyEmail = () => {
  let [searchParams, setSearchParams] = useSearchParams();
  let [isVerified, setIsVerified] = useState(false);

  useEffect(() => {
    const confirmEmail = async () => {
      await verifyEmail();
    };

    confirmEmail();
  }, []);

  const verifyEmail = async () => {
    try {
      var response = await fetch(
        `${process.env.REACT_APP_API_URL}/account/confirmEmail`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            userId: searchParams.get("userId"),
            confirmationCode: searchParams.get("confirmationCode"),
          }),
        }
      );

      if (!response.ok) {
        throw new Error("Unable to verify your email");
      }
      console.log(await response.json());
      setIsVerified(true);
    } catch (error) {
      console.error(error);
      //TODO might need to set the request status.
      setIsVerified(false);
    }
  };

  if (isVerified) {
    return (
      <Alert severity="success">
        Your email was successfully verified! Please click on the following
        <Link href="sign-in"> link </Link> to log in
      </Alert>
    );
  } else {
    return (
      <Alert severity="info">
        We just need to verify your email address before you can access your
        account. Please check your mailbox to confirm you recieve a link to verify your email.
      </Alert>
    );
  }
};

export default VerifyEmail;
