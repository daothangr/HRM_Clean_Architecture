<script setup>
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseDatePicker from '@/components/base/BaseDatePicker.vue'
import BaseDropBox from '@/components/base/BaseDropBox.vue'
import { LEAVE_TYPE_OPTIONS } from '../../constants/option'
import { computed, reactive, watch } from 'vue'
import dayjs from 'dayjs'

const props = defineProps({
  initialData: {
    type: Object,
    default: null,
  },
})

const emit = defineEmits(['close', 'submit'])

const isEditMode = computed(() => Boolean(props.initialData?.id))

/**
 * Get default form data structure
 * @returns {Object} Default form data
 */
const getDefaultFormData = () => ({
  id: '',
  leaveType: '',
  startDate: null,
  endDate: null,
  isFullDay: true,
  startTime: null,
  endTime: null,
  reason: '',
})

const formData = reactive(getDefaultFormData())

/**
 * Calculate total days between start and end date, excluding weekends
 * @returns {number} Number of working days
 */
const calculateTotalDays = () => {
  if (!formData.startDate || !formData.endDate) {
    return 0
  }

  let start = dayjs(formData.startDate)
  let end = dayjs(formData.endDate)
  let count = 0

  if (start.isAfter(end)) {
    return 0
  }

  while (!start.isAfter(end)) {
    // 0 = Sunday, 6 = Saturday
    if (start.day() !== 0 && start.day() !== 6) {
      count++
    }
    start = start.add(1, 'day')
  }

  return count
}

/**
 * Calculate total hours if partial day leave
 * @returns {number|null} Number of hours or null if full day
 */
const calculateTotalHours = () => {
  if (formData.isFullDay || !formData.startTime || !formData.endTime) {
    return null
  }

  // Parse time strings (assuming HH:mm format)
  const start = dayjs(`2000-01-01 ${formData.startTime}`)
  const end = dayjs(`2000-01-01 ${formData.endTime}`)

  return end.diff(start, 'hour', true)
}

const totalDays = computed(() => calculateTotalDays())
const totalHours = computed(() => calculateTotalHours())

/**
 * Close form and emit close event
 */
const handleClose = () => {
  emit('close')
}

/**
 * Submit form with validation and emit data to parent
 */
const handleSubmit = () => {
  // Validation
  if (!formData.leaveType) {
    alert('Vui lòng chọn loại nghỉ phép')
    return
  }

  if (!formData.startDate) {
    alert('Vui lòng chọn ngày bắt đầu')
    return
  }

  if (!formData.endDate) {
    alert('Vui lòng chọn ngày kết thúc')
    return
  }

  if (dayjs(formData.startDate).isAfter(dayjs(formData.endDate))) {
    alert('Ngày bắt đầu phải trước ngày kết thúc')
    return
  }

  if (!formData.isFullDay && (!formData.startTime || !formData.endTime)) {
    alert('Vui lòng nhập giờ bắt đầu và kết thúc cho nghỉ phép nửa ngày')
    return
  }

  if (totalDays.value === 0 && formData.isFullDay) {
    alert('Ngày bắt đầu và kết thúc không hợp lệ (không có ngày làm việc)')
    return
  }

  const payload = {
    ...formData,
    totalDays: totalDays.value,
    totalHours: totalHours.value,
  }

  emit('submit', payload)
  console.log('Leave form submitted with data:', payload)
}

/**
 * Fill form with leave request data or reset to default
 */
const fillFormData = (leave) => {
  if (!leave) {
    Object.assign(formData, getDefaultFormData())
    return
  }

  Object.assign(formData, {
    ...getDefaultFormData(),
    ...leave,
    startDate: leave.startDate ? dayjs(leave.startDate).toDate() : null,
    endDate: leave.endDate ? dayjs(leave.endDate).toDate() : null,
  })
}

/**
 * Watch for changes in initialData and update form accordingly
 */
watch(
  () => props.initialData,
  (newData) => {
    fillFormData(newData)
  },
  { immediate: true },
)
</script>

<template>
  <div class="form-overlay leave-form-overlay">
    <div class="form-modal leave-form-modal">
      <div class="form-header leave-form-header">
        <div class="form-title">{{ isEditMode ? 'Chỉnh sửa đơn nghỉ phép' : 'Tạo đơn nghỉ phép' }}</div>
        <button class="close-button" type="button" aria-label="Đóng form" @click="handleClose">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </div>

      <form class="form-body leave-form-body" @submit.prevent="handleSubmit">
        <div class="form-sections-wrapper leave-form-sections-wrapper">
          <div class="form-sections">
            <section class="form-section">
              <h3 class="form-section-title">Thông tin đơn nghỉ phép</h3>
              <div class="form-grid form-grid--two-columns">
                <BaseDropBox
                  v-model="formData.leaveType"
                  placeholder="Chọn loại nghỉ phép"
                  :options="LEAVE_TYPE_OPTIONS"
                  required
                >
                  Loại nghỉ phép
                </BaseDropBox>

                <div class ="full-day-toggle">
                    <div class="toggle-label">
                        <input
                            v-model="formData.isFullDay"
                            type="checkbox"
                            class="toggle-input"
                        />
                        <span class="toggle-text">Toàn bộ ngày</span>
                    </div>
                </div>

                <BaseDatePicker
                  v-model="formData.startDate"
                  placeholder="Chọn ngày bắt đầu"
                  label="Ngày bắt đầu"
                  required
                />

                <BaseDatePicker
                  v-model="formData.endDate"
                  placeholder="Chọn ngày kết thúc"
                  label="Ngày kết thúc"
                  required
                />

                <BaseInput
                  v-model="formData.startTime"
                  placeholder="HH:mm"
                  type="time"
                  :disabled="formData.isFullDay"
                  required
                >
                  Giờ bắt đầu
                </BaseInput>

                <BaseInput
                  v-model="formData.endTime"
                  placeholder="HH:mm"
                  type="time"
                  :disabled="formData.isFullDay"
                  required
                >
                  Giờ kết thúc
                </BaseInput>

                <BaseInput
                  :value="totalDays"
                  placeholder="Số ngày tính toán tự động"
                  type="number"
                  disabled
                >
                  Tổng số ngày
                </BaseInput>

                <BaseInput
                  v-if="!formData.isFullDay && totalHours !== null"
                  :value="totalHours.toFixed(2)"
                  placeholder="Số giờ tính toán tự động"
                  type="number"
                  disabled
                >
                  Tổng số giờ
                </BaseInput>
              </div>
            </section>

            <section class="form-section">
              <h3 class="form-section-title">Lý do</h3>
              <div class="form-grid">
                <div class="textarea-wrapper">
                  <label class="textarea-label">Ghi chú / Lý do</label>
                  <textarea
                    v-model="formData.reason"
                    placeholder="Nhập lý do nghỉ phép (tùy chọn)"
                    class="base-textarea"
                    rows="4"
                  />
                </div>
              </div>
            </section>
          </div>
        </div>

        <div class="form-actions leave-form-actions">
          <BaseButton backgroundColor="#eef2f7" textColor="#334155" @click="handleClose">
            Hủy
          </BaseButton>
          <BaseButton iconClass="fa-solid fa-paper-plane" @click="handleSubmit">
            {{ isEditMode ? 'Cập nhật đơn' : 'Gửi đơn' }}
          </BaseButton>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>

.close-button {
  width: 36px;
  height: 36px;
  transition: all 0.2s ease;
}

.full-day-toggle {
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
}

.toggle-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  user-select: none;
}

.toggle-input {
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.toggle-text {
  font-size: 14px;
  color: #334155;
  font-weight: 500;
}

.textarea-wrapper {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.textarea-label {
  font-size: 14px;
  font-weight: 600;
  color: #334155;
}

.base-textarea {
  padding: 10px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-family: inherit;
  font-size: 14px;
  color: #1e293b;
  resize: vertical;
  transition: all 0.2s ease;
}

.base-textarea:focus {
  outline: none;
  border-color: #4387ee;
  box-shadow: 0 0 0 3px rgba(67, 135, 238, 0.1);
}

@media (max-width: 768px) {
  .leave-form-overlay {
    --form-overlay-padding: 12px;
  }

  .leave-form-actions {
    --form-actions-padding: 16px 16px;
  }
}
</style>
