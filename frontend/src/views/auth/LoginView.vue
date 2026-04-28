<script setup>
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const formData = reactive({
  email: '',
  password: '',
})

const isLoading = ref(false)
const errorMessage = ref('')

const redirectPath = computed(() => {
  const value = route.query.redirect

  if (typeof value === 'string' && value.startsWith('/')) {
    return value
  }

  return '/dashboard'
})

const handleLogin = async () => {
  if (isLoading.value) {
    return
  }

  errorMessage.value = ''
  isLoading.value = true

  try {
    await authStore.login({
      email: formData.email.trim(),
      password: formData.password,
    })

    await router.replace(redirectPath.value)
  } catch (error) {
    errorMessage.value = error?.response?.data?.message || 'Không thể đăng nhập. Vui lòng kiểm tra email và mật khẩu.'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <section class="login-page">
    <div class="login-card">
      <div class="login-card__header">
        <div class="login-card__icon" aria-hidden="true">
          <i class="fa-solid fa-building"></i>
        </div>
        <div>
          <h1>HRM Portal</h1>
          <p>Đăng nhập hệ thống</p>
        </div>
      </div>

      <div class="login-card__demo">
        <span>Email: Admin@hr.com</span>
        <span>Mật khẩu: Admin@123</span>
      </div>

      <form class="login-form" @submit.prevent="handleLogin">
        <BaseInput
          v-model="formData.email"
          placeholder="Nhập email"
          type="email"
          autocomplete="email"
          required
        >
          Email
        </BaseInput>

        <BaseInput
          v-model="formData.password"
          placeholder="Nhập mật khẩu"
          type="password"
          autocomplete="current-password"
          required
        >
          Mật khẩu
        </BaseInput>

        <div v-if="errorMessage" class="login-form__error">
          {{ errorMessage }}
        </div>

        <BaseButton class="login-form__submit" iconClass="fa-solid fa-right-to-bracket" @click="handleLogin">
          {{ isLoading ? 'Đang đăng nhập...' : 'Đăng nhập' }}
        </BaseButton>
      </form>
    </div>
  </section>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 24px;
  background: linear-gradient(135deg, #f7f9fc 0%, #eef3f8 100%);
}

.login-card {
  width: min(420px, 100%);
  border-radius: 16px;
  padding: 24px;
  background: #ffffff;
  border: 1px solid #e5eaf2;
  box-shadow: 0 16px 40px rgba(15, 23, 42, 0.08);
}

.login-card__header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
}

.login-card__icon {
  width: 42px;
  height: 42px;
  display: grid;
  place-items: center;
  border-radius: 12px;
  color: #ffffff;
  background: var(--color-branch-primary);
}

.login-card__header h2 {
  display: none;
}

.login-card__header h1 {
  margin: 0;
  font-size: 20px;
  color: #0f172a;
}

.login-card__header p {
  margin: 4px 0 0;
  color: #64748b;
  font-size: 13px;
}

.login-card__demo {
  display: grid;
  gap: 4px;
  margin-bottom: 16px;
  padding: 12px 14px;
  border-radius: 12px;
  background: #f8fbff;
  border: 1px solid #dbe7fb;
  color: #1d4ed8;
  font-size: 13px;
}

.login-form {
  display: grid;
  gap: 14px;
}

.login-form__error {
  padding: 10px 12px;
  border-radius: 10px;
  background: #fff1f2;
  border: 1px solid #fecdd3;
  color: #be123c;
  font-size: 13px;
}

.login-form__submit {
  width: 100%;
}

@media (max-width: 640px) {
  .login-page {
    padding: 16px;
  }

  .login-card {
    padding: 20px;
  }
}
</style>