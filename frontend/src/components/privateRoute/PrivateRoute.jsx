import React from 'react';
import { Navigate } from 'react-router-dom';
import { getAccessToken, getRoleFromToken, isRoleAllowed } from '../../services/api';
import { useAuth } from '../../context/authContext';

export const PrivateRoute = ({ children }) => {
    const { user, loading } = useAuth();

  if (!user) {
    return <Navigate to="/login" />;
  }

  const role = getRoleFromToken(user.access_token);
  if (!isRoleAllowed(role)) {
    return <Navigate to="/forbidden" />;
  }

  return children;
};