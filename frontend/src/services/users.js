// services/users.js
import { authApiRequest } from './api';

/**
 * Поиск пользователей по запросу
 * @param {string} query - строка поиска (email или имя)
 * @returns {Promise<Array>} массив пользователей
 */
export const searchUsers = async (query = '') => {
  const response = await authApiRequest(`/api/users/search?query=${encodeURIComponent(query)}`, {
    method: 'GET',
  });
  if (!response.ok) {
    throw new Error('Не удалось загрузить пользователей');
  }
  return response.json();
};

/**
 * Получение пользователя по ID
 * @param {string} id - UUID пользователя
 * @returns {Promise<Object>} объект пользователя
 */
export const getUserById = async (id) => {
  const response = await authApiRequest(`/api/users/${id}`, { method: 'GET' });
  if (!response.ok) {
    throw new Error('Не удалось загрузить данные пользователя');
  }
  return response.json();
};

export const addUserRole = async (userId, role) => {
  const response = await authApiRequest(`/api/users/${userId}/role?role=${encodeURIComponent(role)}`, {
    method: 'POST',
  });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось добавить роль');
  }
  return response.json(); // предположительно возвращает что-то или просто ok
};

export const removeUserRole = async (userId, role) => {
  const response = await authApiRequest(`/api/users/${userId}/role?role=${encodeURIComponent(role)}`, {
    method: 'DELETE',
  });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось удалить роль');
  }
  return response.json();
};

/**
 * Заблокировать пользователя
 * @param {string} userId
 * @returns {Promise<Object>}
 */
export const blockUser = async (userId) => {
  const response = await authApiRequest(`/api/auth/block/${userId}`, {
    method: 'POST',
  });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось заблокировать пользователя');
  }
  return response.json();
};

/**
 * Разблокировать пользователя
 * @param {string} userId
 * @returns {Promise<Object>}
 */
export const unblockUser = async (userId) => {
  const response = await authApiRequest(`/api/auth/unblock/${userId}`, {
    method: 'POST',
  });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось разблокировать пользователя');
  }
  return response.json();
};

// (Опционально) функции для управления ролями можно добавить позже
// export const addUserRole = async (userId, role) => { ... }
// export const removeUserRole = async (userId, role) => { ... }