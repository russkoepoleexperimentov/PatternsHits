import React, { createContext, useState, useEffect, useContext } from 'react';
import { getUser, login, logout } from '../services/authService';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initAuth = async () => {
      try {
        const currentUser = await getUser();
        setUser(currentUser);
      } catch (error) {
        console.error('Auth init error', error);
      } finally {
        setLoading(false);
      }
    };
    initAuth();
  }, []);

  const loginHandler = () => {
    login();
  };

  const logoutHandler = () => {
    logout();
  };

  const value = {
    user,
    loading,
    login: loginHandler,
    logout: logoutHandler,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => useContext(AuthContext);