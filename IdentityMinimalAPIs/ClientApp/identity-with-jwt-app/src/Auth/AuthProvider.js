import React, { createContext, useContext, useState } from "react";

const Context = createContext();

export default function AuthProvider({ children }) {
  const [userData, setUserData] = useState(null);
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  return (
    <Context.Provider
      value={{ isLoggedIn, setIsLoggedIn, userData, setUserData }}
    >
      {children}
    </Context.Provider>
  );
}

export const useAuth = () => {
  return useContext(Context);
};
