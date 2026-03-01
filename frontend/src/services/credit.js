// services/credit.js
import { creditApiRequest } from './api';

/**
 * Получить список всех тарифов
 * @returns {Promise<Array>} массив TariffDto
 */
export const getTariffs = async () => {
  const response = await creditApiRequest('/api/tariffs', { method: 'GET' });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось загрузить тарифы');
  }
  return response.json();
};

/**
 * Создать новый тариф
 * @param {Object} data - поля: name, interestRate, maxAmount, maxTermDays
 * @returns {Promise<Object>} созданный TariffDto
 */
export const createTariff = async (data) => {
  const response = await creditApiRequest('/api/tariffs', {
    method: 'POST',
    body: JSON.stringify(data),
  });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось создать тариф');
  }
  return response.json();
};

/**
 * Получить тариф по ID
 * @param {string} id
 * @returns {Promise<Object>} TariffDto
 */
export const getTariffById = async (id) => {
  const response = await creditApiRequest(`/api/tariffs/${id}`, { method: 'GET' });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось загрузить тариф');
  }
  return response.json();
};

/**
 * Обновить существующий тариф
 * @param {string} id
 * @param {Object} data - поля: name, interestRate, maxAmount, maxTermDays
 * @returns {Promise<Object>} обновлённый TariffDto
 */
export const updateTariff = async (id, data) => {
  const response = await creditApiRequest(`/api/tariffs/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data),
  });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось обновить тариф');
  }
  return response.json();
};

/**
 * Удалить тариф по ID
 * @param {string} id
 * @returns {Promise<void>}
 */
export const deleteTariff = async (id) => {
  const response = await creditApiRequest(`/api/tariffs/${id}`, { method: 'DELETE' });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось удалить тариф');
  }
  // при успехе возвращается пустой ответ с кодом 200
};