export const onRequest = (config) => {
  const storedToken = localStorage.getItem('accessToken') || localStorage.getItem('token');

  if (storedToken) {
    config.headers = config.headers || {};
    config.headers.Authorization = `Bearer ${storedToken}`;
  }

  return config;
};
