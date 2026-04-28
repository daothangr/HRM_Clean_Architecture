import axios from './axios'

export const getDepartments = (params = {}) => {
  return axios.get('/departments', { params })
}

export const getDepartmentById = (id) => {
  return axios.get(`/departments/${id}`)
}

export const createDepartment = (data) => {
  return axios.post('/departments', data)
}

export const updateDepartment = (id, data) => {
  return axios.put(`/departments/${id}`, data)
}

export const deleteDepartment = (id) => {
  return axios.delete(`/departments/${id}`)
}