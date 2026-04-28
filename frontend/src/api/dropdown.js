import axios from './axios';

export const getDropdownOptions = async (route) => {
  const response = await axios.get(route);
  return response.data;
};