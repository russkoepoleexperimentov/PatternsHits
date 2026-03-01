// services/api.js
import { jwtDecode } from 'jwt-decode';

// Функции для работы с токенами (общие)
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

const getRoleFromToken = (token) => {
  try {
    const decoded = jwtDecode(token);
    return decoded.role; // предполагается поле "role"
  } catch {
    return null;
  }
};

const isRoleAllowed = (role) => role !== 'Customer';

// Фабрика для создания API-клиента с заданным baseURL
const createApiRequest = (baseURL) => {
  return async function apiRequest(endpoint, options = {}) {
    const url = `${baseURL}${endpoint}`;
    const accessToken = getAccessToken();

    const headers = {
      'Content-Type': 'application/json',
      ...options.headers,
    };

    if (accessToken) {
      headers.Authorization = `Bearer ${accessToken}`;
    }

    let response = await fetch(url, { ...options, headers });

    // Попытка обновить токен при 401
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
  };
};

// Функция обновления токена (использует baseURL Auth, так как endpoint принадлежит Auth)
async function refreshAccessToken() {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return null;

  const authBaseURL = process.env.REACT_APP_AUTH_API_URL || 'http://localhost:5000';
  try {
    const response = await fetch(`${authBaseURL}/api/auth/refresh?token=${refreshToken}`, {
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

// Создаём два экземпляра: для Auth и для Core
const authApiRequest = createApiRequest(process.env.REACT_APP_AUTH_API_URL || 'http://localhost:5000');
const coreApiRequest = createApiRequest(process.env.REACT_APP_CORE_API_URL || 'http://localhost:5001');

export {
  authApiRequest,
  coreApiRequest,
  setTokens,
  removeTokens,
  getAccessToken,
  getRefreshToken,
  getRoleFromToken,
  isRoleAllowed,
};