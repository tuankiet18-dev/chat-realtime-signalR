const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5185';

export const getApiUrl = () => API_BASE_URL;

export const apiCall = async (endpoint, options = {}) => {
  const token = localStorage.getItem('chat_token');
  const headers = {
    'Content-Type': 'application/json',
    ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
    ...options.headers,
  };

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  let data;
  try {
    data = await response.json();
  } catch {
    data = null;
  }

  if (!response.ok) {
    throw new Error(data?.error || data?.message || 'API request failed');
  }

  return data;
};

export const uploadFile = async (file) => {
  const token = localStorage.getItem('chat_token');
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(`${API_BASE_URL}/api/uploads`, {
    method: 'POST',
    headers: {
      ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
    },
    body: formData,
  });

  let data;
  try {
    data = await response.json();
  } catch {
    data = null;
  }

  if (!response.ok) {
    throw new Error(data?.error || data?.message || 'Upload failed');
  }

  return data;
};
