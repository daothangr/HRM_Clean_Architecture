import { useAuthStore } from '@/stores/auth'
import { hasAnyRole } from '@/utils/auth'

export const setupRouterGuards = (router) => {
  router.beforeEach((to) => {
    const authStore = useAuthStore()
    document.title = `HRM - ${to.meta.title || 'Portal'}`

    if (to.meta.guestOnly && authStore.isAuthenticated) {
      return { path: '/profile' }
    }

    if (to.matched.some(record => record.meta.requiresAuth) && !authStore.isAuthenticated) {
      return {
        path: '/login'
      }
    }

    const roleProtectedRecord = [...to.matched]
      .reverse()
      .find(record => Array.isArray(record.meta?.roles) && record.meta.roles.length > 0)

    if (roleProtectedRecord && !hasAnyRole(authStore.roles, roleProtectedRecord.meta.roles)) {
      return { path: '/profile' }
    }

    return true
  })
}