<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'
import { upsertAttendance } from '@/api/attendance'

const authStore = useAuthStore()
const toast = useToast()

const isSubmitting = ref(false)
const clockNow = ref(new Date())
let clockTimerId = null

const employeeId = computed(() => authStore.user?.employeeId ?? '--')
const fullName = computed(() => authStore.user?.fullName ?? '--')
const employeeCode = computed(() => authStore.user?.employeeCode ?? '--')

const pad = (value) => String(value).padStart(2, '0')

const toLocalDateTimeString = (date) => {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`
}

const toTimeOnlyString = (date) => {
  return `${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`
}

const toDisplayDateTime = (date) => {
  return new Intl.DateTimeFormat('vi-VN', {
    dateStyle: 'full',
    timeStyle: 'medium',
  }).format(date)
}

const syncClock = () => {
  clockNow.value = new Date()
}

const handleCheckIn = async () => {
  if (!employeeId.value) {
    toast.error('Không tìm thấy thông tin nhân viên để chấm công')
    return
  }

  const currentTime = new Date()
  const attendancePayload = {
    employeeId: employeeId.value,
    date: toLocalDateTimeString(currentTime),
    attendanceTime: toTimeOnlyString(currentTime),
  }

  isSubmitting.value = true

  try {
    await upsertAttendance(attendancePayload)
    toast.success(`Đã chấm công lúc ${toTimeOnlyString(currentTime)}`)
  } catch (error) {
    console.error('Failed to check in:', error)
    toast.error('Không thể chấm công. Vui lòng thử lại sau.')
  } finally {
    isSubmitting.value = false
  }
}

onMounted(() => {
  syncClock()
  clockTimerId = window.setInterval(syncClock, 1000)
})

onBeforeUnmount(() => {
  if (clockTimerId) {
    window.clearInterval(clockTimerId)
  }
})
</script>

<template>
  <section class="check-in-page">
    <div class="check-in-card">
      <h1 class="check-in-card__title">Chấm công</h1>
      <p class="check-in-card__subtitle">Nhấn nút bên dưới để gửi thời gian chấm công hiện tại lên hệ thống.</p>

      <div class="check-in-clock">
        <span class="check-in-clock__label">Thời gian hiện tại</span>
        <strong class="check-in-clock__value">{{ toDisplayDateTime(clockNow) }}</strong>
      </div>

      <div class="check-in-meta">
        <div>
          <span class="check-in-meta__label">Mã nhân viên</span>
          <strong class="check-in-meta__value">{{ employeeCode || '--' }}</strong>
        </div>
        <div>
          <span class="check-in-meta__label">Tên nhân viên</span>
          <strong class="check-in-meta__value">{{ fullName || '--' }}</strong>
        </div>

      </div>

      <button
        class="check-in-button"
        type="button"
        :disabled="isSubmitting"
        @click="handleCheckIn"
      >
        <span v-if="isSubmitting">Đang chấm công...</span>
        <span v-else>Chấm công ngay</span>
      </button>
    </div>
  </section>
</template>

<style scoped>
.check-in-page {
  min-height: calc(100vh - 80px);
  display: grid;
  place-items: center;
  padding: 24px;
  background: var(--color-bg-default);
}

.check-in-card {
  width: min(100%, 640px);
  padding: 28px;
  border: 1px solid rgba(67, 135, 238, 0.18);
  border-radius: 20px;
  background: #ffffff;
  box-shadow: 0 8px 24px rgba(18, 34, 48, 0.08);
}

.check-in-card__title {
  margin: 0;
  font-size: 28px;
  line-height: 1.2;
  color: var(--color-text-primary);
}

.check-in-card__subtitle {
  margin: 12px 0 0;
  font-size: 14px;
  line-height: 1.6;
  color: var(--color-text-secondary);
}

.check-in-clock {
  margin-top: 28px;
  padding: 16px 18px;
  border-radius: 14px;
  background: var(--color-status-focus);
  color: var(--color-text-primary);
}

.check-in-clock__label,
.check-in-meta__label {
  display: block;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.check-in-clock__value {
  display: block;
  margin-top: 10px;
  font-size: 18px;
  line-height: 1.35;
}

.check-in-meta {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  margin-top: 20px;
}

.check-in-meta > div {
  padding: 14px 16px;
  border-radius: 14px;
  background: #fafafa;
  border: 1px solid rgba(18, 34, 48, 0.08);
}

.check-in-meta__value {
  display: block;
  margin-top: 8px;
  font-size: 16px;
  color: var(--color-text-primary);
}

.check-in-button {
  width: 100%;
  margin-top: 24px;
  border: none;
  border-radius: 14px;
  padding: 14px 20px;
  font-size: 16px;
  font-weight: 700;
  color: #ffffff;
  background: var(--color-branch-color, var(--color-branch-primary));
  box-shadow: 0 10px 24px rgba(67, 135, 238, 0.24);
  cursor: pointer;
  transition: opacity 0.15s ease, transform 0.15s ease;
}

.check-in-button:hover:not(:disabled) {
  transform: translateY(-1px);
}

.check-in-button:disabled {
  cursor: not-allowed;
  opacity: 0.7;
  box-shadow: none;
}

@media (max-width: 640px) {
  .check-in-page {
    padding: 16px;
  }

  .check-in-card {
    padding: 20px;
    border-radius: 16px;
  }

  .check-in-meta {
    grid-template-columns: 1fr;
  }
}
</style>
