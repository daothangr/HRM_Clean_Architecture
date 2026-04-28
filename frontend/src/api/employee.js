import axios from './axios';

export const getEmployees = (params = {}) => {
  return axios.get('/Employees', { params });
};

export const getEmployeeById = (id) => {
  return axios.get(`/employees/${id}`);
};

export const createEmployee = (employeeData) => {
  return axios.post('/employees', employeeData);
};

export const updateEmployee = (id, employeeData) => {
  return axios.put(`/employees/${id}`, employeeData);
};

export const deleteEmployee = (id) => {
  return axios.delete(`/employees/${id}`);
};
