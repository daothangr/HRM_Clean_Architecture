import { reactive } from 'vue'

export function useFormState() {
  const formState = reactive({})

  const setFieldState = (field, status, message = '') => {
    formState[field] = {
      status,
      message
    }
  }

  const clearFieldState = (field) => {
    delete formState[field]
  }

  const clearAllStates = () => {
    Object.keys(formState).forEach(key => delete formState[key])
  }

  return {
    formState,
    setFieldState,
    clearFieldState,
    clearAllStates
  }
}