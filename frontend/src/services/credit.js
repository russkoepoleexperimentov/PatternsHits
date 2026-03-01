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

/**
 * Получить список кредитов (для сотрудника)
 * @param {string} [userId] - опциональный фильтр по пользователю
 * @returns {Promise<Array>} массив CreditDto
 */
export const getCredits = async (userId) => {
  const url = userId ? `/api/credits?userId=${encodeURIComponent(userId)}` : '/api/credits';
  const response = await creditApiRequest(url, { method: 'GET' });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось загрузить кредиты');
  }
  return response.json();
};

/**
 * Получить кредит по ID
 * @param {string} id
 * @returns {Promise<Object>} CreditDto
 */
export const getCreditById = async (id) => {
  const response = await creditApiRequest(`/api/credits/${id}`, { method: 'GET' });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось загрузить кредит');
  }
  return response.json();
};

/**
 * Одобрить кредит
 * @param {string} id
 * @param {Object} data - { approvedAmount, comment }
 * @returns {Promise<Object>} обновлённый CreditDto
 */
export const approveCredit = async (id, data) => {
  const response = await creditApiRequest(`/api/credits/${id}/approve`, {
    method: 'PATCH',
    body: JSON.stringify(data),
  });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось одобрить кредит');
  }
  return response.json();
};

/**
 * Отклонить кредит
 * @param {string} id
 * @param {Object} data - { reason }
 * @returns {Promise<Object>} обновлённый CreditDto
 */
export const rejectCredit = async (id, data) => {
  const response = await creditApiRequest(`/api/credits/${id}/reject`, {
    method: 'PATCH',
    body: JSON.stringify(data),
  });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось отклонить кредит');
  }
  return response.json();
};

/**
 * Получить платежи по кредиту (опционально)
 * @param {string} creditId
 * @returns {Promise<Array>} массив PaymentDto
 */
export const getCreditPayments = async (creditId) => {
  const response = await creditApiRequest(`/api/credits/${creditId}/payments`, { method: 'GET' });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Не удалось загрузить платежи');
  }
  return response.json();
};