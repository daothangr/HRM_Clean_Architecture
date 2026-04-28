import axios from 'axios';
import { onRequest } from '@/api/interceptors/requestInterceptor';
import { onResponseError, onResponseSuccess } from '@/api/interceptors/responseInterceptor';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || '/api';

const instance = axios.create({
  baseURL: apiBaseUrl,
  timeout: 5000,
});

instance.interceptors.request.use(onRequest);
instance.interceptors.response.use(onResponseSuccess, onResponseError);

export default instance;