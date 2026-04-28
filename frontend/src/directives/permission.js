import { useAuthStore } from '@/stores/auth'
import { hasAnyRole } from '@/utils/auth'
import { normalizeToArray } from '@/utils/formatters'

const syncPermissionState = (el, binding) => {
  const authStore = useAuthStore()
  const roles = normalizeToArray(binding.value)
  const canAccess = roles.length === 0 || hasAnyRole(authStore.roles, roles)

  if (canAccess) {
    el.style.display = ''
    return
  }

  el.style.display = 'none'
  el.setAttribute('aria-hidden', 'true')
}

const permissionDirective = {
  mounted(el, binding) {
    syncPermissionState(el, binding)
  },
  updated(el, binding) {
    syncPermissionState(el, binding)
  },
}

export default permissionDirective
