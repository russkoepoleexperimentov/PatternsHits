import React, { createContext, useState, useContext, useEffect } from 'react';
import {
  apiRequest,
  setTokens,
  removeTokens,
  getAccessToken,
  getRoleFromToken,
  isRoleAllowed,
} from '../services/api';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadUser = async () => {
      const token = getAccessToken();
      if (token) {
        const role = getRoleFromToken(token);
        if (!isRoleAllowed(role)) {
          // Роль Customer – удаляем токены и не загружаем пользователя
          removeTokens();
          setLoading(false);
          return;
        }
        try {
          const response = await apiRequest('/api/users', { method: 'GET' });
          if (response.ok) {
            const userData = await response.json();
            setUser(userData);
          } else {
            removeTokens();
          }
        } catch {
          removeTokens();
        }
      }
      setLoading(false);
    };
    loadUser();
  }, []);

  const login = async (email, password) => {
    const response = await fetch(`${process.env.REACT_APP_AUTH_API_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });

    if (response.ok) {
      const data = await response.json();
      const role = getRoleFromToken(data.accessToken);
      if (!isRoleAllowed(role)) {
        // Роль Customer – не пускаем
        removeTokens();
        return { success: false, error: 'Доступ запрещён для данной роли' };
      }
      setTokens(data.accessToken, data.refreshToken);
      const userResponse = await apiRequest('/api/users', { method: 'GET' });
      if (userResponse.ok) {
        const userData = await userResponse.json();
        setUser(userData);
        return { success: true };
      } else {
        removeTokens();
        return { success: false, error: 'Не удалось загрузить данные пользователя' };
      }
    } else {
      const errorData = await response.json().catch(() => ({}));
      return { success: false, error: errorData.message || 'Ошибка входа' };
    }
  };

  const register = async (email, password, credentials) => {
    const response = await fetch(`${process.env.REACT_APP_AUTH_API_URL}/api/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password, credentials }),
    });

    if (response.ok) {
      return { success: true };
    } else {
      const errorData = await response.json().catch(() => ({}));
      return { success: false, error: errorData.message || 'Ошибка регистрации' };
    }
  };

  const logout = async () => {
    try {
      await apiRequest('/api/auth/logout', { method: 'POST' });
    } finally {
      removeTokens();
      setUser(null);
    }
  };

  const updateProfile = async (data) => {
    const response = await apiRequest('/api/users', {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    if (response.ok) {
      const userResponse = await apiRequest('/api/users', { method: 'GET' });
      const updatedUser = await userResponse.json();
      setUser(updatedUser);
      return { success: true };
    } else {
      const error = await response.json().catch(() => ({}));
      return { success: false, error: error.message || 'Ошибка обновления' };
    }
  };

  const changePassword = async (oldPassword, newPassword) => {
    const response = await apiRequest('/api/auth/change-password', {
      method: 'POST',
      body: JSON.stringify({ oldPassword, newPassword }),
    });
    if (response.ok) {
      return { success: true };
    } else {
      const error = await response.json().catch(() => ({}));
      return { success: false, error: error.message || 'Ошибка смены пароля' };
    }
  };

  const value = {
    user,
    login,
    register,
    logout,
    updateProfile,
    changePassword,
    loading,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => useContext(AuthContext);