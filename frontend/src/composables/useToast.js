import { ref } from 'vue'
import { STATUS_NOTIFY } from '@/constants/enum'

const toasts = ref([])
let nextToastId = 1

export function useToast() {
  const show = (message, type = STATUS_NOTIFY.SUCCESS, duration = 3000) => {
    const id = nextToastId++

    toasts.value.push({ id, message, type })

    setTimeout(() => {
      remove(id)
    }, duration)
  }

  const remove = (id) => {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }

  return {
    toasts,
    remove,
    show,
    success: (msg) => show(msg, STATUS_NOTIFY.SUCCESS),
    error: (msg) => show(msg, STATUS_NOTIFY.ERROR),
    warning: (msg) => show(msg, STATUS_NOTIFY.WARNING)
  }
}