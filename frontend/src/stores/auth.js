import { defineStore } from 'pinia'
import { login as loginRequest } from '@/api/auth'

const ACCESS_TOKEN_KEY = 'accessToken'
const REFRESH_TOKEN_KEY = 'refreshToken'
const USER_INFO_KEY = 'authUser'

const readStoredUser = () => {
  const storedUser = localStorage.getItem(USER_INFO_KEY)

  if (!storedUser) {
    return null
  }

  try {
    return JSON.parse(storedUser)
  } catch (error) {
    return null
  }
}

const clearStoredSession = () => {
  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
  localStorage.removeItem(USER_INFO_KEY)
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    accessToken: localStorage.getItem(ACCESS_TOKEN_KEY) || '',
    refreshToken: localStorage.getItem(REFRESH_TOKEN_KEY) || '',
    user: readStoredUser(),
  }),

  getters: {
    isAuthenticated: (state) => Boolean(state.accessToken),
    fullName: (state) => state.user?.fullName || '',
    email: (state) => state.user?.email || '',
    roles: (state) => state.user?.roles || [],
    employeeCode: (state) => state.user?.employeeCode || '',
  },

  actions: {
    setSession(payload) {
      this.accessToken = payload.accessToken || ''
      this.refreshToken = payload.refreshToken || ''
      this.user = {
        employeeId: payload.employeeId,
        fullName: payload.fullName,
        email: payload.email,
        roles: payload.roles || [],
        employeeCode: payload.employeeCode || '',
      }

      localStorage.setItem(ACCESS_TOKEN_KEY, this.accessToken)
      localStorage.setItem(REFRESH_TOKEN_KEY, this.refreshToken)
      localStorage.setItem(USER_INFO_KEY, JSON.stringify(this.user))
    },

    async login(credentials) {
      const response = await loginRequest(credentials)
      const payload = response.data ?? response

      this.setSession(payload)

      return payload
    },

    logout() {
      this.accessToken = ''
      this.refreshToken = ''
      this.user = null
      clearStoredSession()
    },
  },
})