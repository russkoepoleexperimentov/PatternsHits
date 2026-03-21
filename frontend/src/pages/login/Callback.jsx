import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { loginCallback } from '../../services/authService';

const Callback = () => {
  const navigate = useNavigate();

  useEffect(() => {
    const handleCallback = async () => {
      try {
        await loginCallback();
        navigate('/');
      } catch (error) {
        console.error('Callback error', error);
        navigate('/login');
      }
    };
    handleCallback();
  }, [navigate]);

  return <div>Loading...</div>;
};

export default Callback;