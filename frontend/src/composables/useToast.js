import { ref } from 'vue'

const toasts = ref([])
let nextToastId = 1

export function useToast() {
  const show = (message, type = 'success', duration = 3000) => {
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
    success: (msg) => show(msg, 'success'),
    error: (msg) => show(msg, 'error'),
    warning: (msg) => show(msg, 'warning')
  }
}