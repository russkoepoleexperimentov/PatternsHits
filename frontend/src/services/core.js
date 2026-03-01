// services/core.js
import { coreApiRequest } from './api';

export const getAccounts = async () => {
  const response = await coreApiRequest('/api/accounts/employee', { method: 'GET' });
  if (!response.ok) {
    throw new Error('Failed to fetch accounts');
  }
  return response.json(); // предполагаем, что возвращается массив объектов AccountDto
};

export const getAccountById = async (id) => {
  const response = await coreApiRequest(`/api/accounts/employee/${id}`, { method: 'GET' });
  if (!response.ok) {
    throw new Error('Failed to fetch account');
  }
  return response.json();
};

export const deleteAccount = async (id) => {
  const response = await coreApiRequest(`/api/accounts/employee/${id}`, { method: 'DELETE' });
  if (!response.ok) {
    throw new Error('Failed to delete account');
  }
  return response.json();
};

export const deposit = async (id, { amount, description }) => {
    const body = {
        sourceId: null,
        sourceType: "RealWorld",
        targetId: id,
        targetType: "Account",
        amount,
        description
    }

  const response = await coreApiRequest(`/api/transactions`, { method: 'POST', body: JSON.stringify(body)  });
  if (!response.ok) {
    throw new Error('Failed to deposit');
  }
  return response.json();
};

export const withdraw = async (id, { amount, description }) => {
    const body = {
        sourceId: id,
        sourceType: "Account",
        targetId: null,
        targetType: "RealWorld",
        amount,
        description
    }

  const response = await coreApiRequest(`/api/transactions`, { method: 'POST', body: JSON.stringify(body)  });
  if (!response.ok) {
    throw new Error('Failed to deposit');
  }
  return response.json();
};

export const getAccountTransactions = async (accountId, from, to) => {
  let url = `/api/accounts/employee/${accountId}/transactions`;
  const params = new URLSearchParams();
  if (from) params.append('from', from.toISOString());
  if (to) params.append('to', to.toISOString());
  if (params.toString()) url += `?${params.toString()}`;
  
  const response = await coreApiRequest(url, { method: 'GET' });
  if (!response.ok) {
    throw new Error('Failed to fetch transactions');
  }
  return response.json();
};