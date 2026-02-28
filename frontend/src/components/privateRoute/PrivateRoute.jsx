import React from 'react';
import { Navigate } from 'react-router-dom';
import { getAccessToken, getRoleFromToken, isRoleAllowed } from '../../services/api';

export const PrivateRoute = ({ children }) => {
  const token = getAccessToken();

  if (!token) {
    return <Navigate to="/login" />;
  }

  const role = getRoleFromToken(token);
  if (!isRoleAllowed(role)) {
    return <Navigate to="/forbidden" />;
  }

  return children;
};