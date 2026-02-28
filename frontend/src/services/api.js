// src/services/api.js
import { jwtDecode } from 'jwt-decode';

const API_BASE_URL = process.env.REACT_APP_AUTH_API_URL || 'http://localhost:5000';

const getAccessToken = () => localStorage.getItem('accessToken');
const getRefreshToken = () => localStorage.getItem('refreshToken');

const setTokens = (accessToken, refreshToken) => {
  localStorage.setItem('accessToken', accessToken);
  if (refreshToken) localStorage.setItem('refreshToken', refreshToken);
};

const removeTokens = () => {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
};

// Новая функция: извлечение роли из токена
const getRoleFromToken = (token) => {
  try {
    const decoded = jwtDecode(token);
    return decoded.role; // предполагается, что в токене есть поле "role"
  } catch {
    return null;
  }
};

// Проверка, разрешена ли роль (не Customer)
const isRoleAllowed = (role) => role !== 'Customer';

async function apiRequest(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;
  const accessToken = getAccessToken();

  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  if (accessToken) {
    headers.Authorization = `Bearer ${accessToken}`;
  }

  let response = await fetch(url, { ...options, headers });

  if (response.status === 401 && getRefreshToken()) {
    const newTokens = await refreshAccessToken();
    if (newTokens) {
      headers.Authorization = `Bearer ${newTokens.accessToken}`;
      response = await fetch(url, { ...options, headers });
    } else {
      removeTokens();
      window.location.href = '/login';
      throw new Error('Session expired');
    }
  }

  return response;
}

async function refreshAccessToken() {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return null;

  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/refresh?token=${refreshToken}`, {
      method: 'POST',
    });
    if (response.ok) {
      const data = await response.json();
      setTokens(data.accessToken, data.refreshToken || refreshToken);
      return { accessToken: data.accessToken };
    } else {
      removeTokens();
      return null;
    }
  } catch {
    removeTokens();
    return null;
  }
}

export {
  apiRequest,
  setTokens,
  removeTokens,
  getAccessToken,
  getRefreshToken,
  getRoleFromToken,
  isRoleAllowed,
};