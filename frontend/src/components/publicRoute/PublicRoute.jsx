import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../context/authContext';

export const PublicRoute = ({ children }) => {
  const { user } = useAuth();
  // Если пользователь уже авторизован – перенаправляем на главную
  return user ? <Navigate to="/" /> : children;
};