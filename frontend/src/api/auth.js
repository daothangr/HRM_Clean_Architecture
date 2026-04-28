import axios from './axios'

export const login = (credentials) => {
  return axios.post('/Auth/login', credentials)
}