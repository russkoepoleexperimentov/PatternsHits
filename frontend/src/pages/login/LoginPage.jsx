import React from 'react';
import { useAuth } from '../../context/authContext';

export const LoginPage = () => {
  const { login } = useAuth();
  return (
    <div>
      <h1>Login</h1>
      <button onClick={login}>Sign in with IdentityServer</button>
    </div>
  );
};