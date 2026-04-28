import { useToast } from '@/composables/useToast';

const toast = useToast();

export const getErrorMessage = (error) => {
  const responseData = error?.response?.data;

  if (responseData?.message) {
    return responseData.message;
  }

  if (responseData?.title) {
    return responseData.title;
  }

  if (Array.isArray(responseData?.errors) && responseData.errors.length > 0) {
    return responseData.errors;
  }

  if (responseData?.errors && typeof responseData.errors === 'object') {
    const errorGroup = Object.values(responseData.errors).find(
      (item) => Array.isArray(item) && item.length > 0,
    );

    if (errorGroup) {
      return errorGroup;
    }
  }

  if (error?.code === 'ECONNABORTED') {
    return 'Request timeout. Vui long thu lai.';
  }

  if (!error?.response) {
    return 'Không thể kết nối đến server. Vui lòng kiểm tra kết nối mạng và thử lại.';
  }

  return 'Đã có lỗi xảy ra. Vui lòng thử lại.';
};

export const onResponseSuccess = (response) => response;

export const onResponseError = (error) => {
  if (!error?.config?.skipErrorToast) {
    console.log('Error response:', getErrorMessage(error));
  }

  return Promise.reject(error);
};
