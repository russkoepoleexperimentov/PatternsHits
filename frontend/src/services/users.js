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

// (Опционально) функции для управления ролями можно добавить позже
// export const addUserRole = async (userId, role) => { ... }
// export const removeUserRole = async (userId, role) => { ... }