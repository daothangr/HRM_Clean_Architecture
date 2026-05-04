<script setup>
import { computed, useAttrs } from "vue";
import dayjs from "dayjs";
import { STATUS_NOTIFY } from "@/constants/enum";

defineOptions({ inheritAttrs: false });
const attrs = useAttrs();
// =======================
// Props & Emits Region
// =======================
const props = defineProps({
  disable: {
    type: Boolean,
    default: false,
  },
  label: String,
  required: Boolean,
  placeholder: String,
  modelValue: {
    type: Date,
    default: null,
  },
  status: {
    type: String,
    default: STATUS_NOTIFY.DEFAULT,
    validator: (value) => Object.values(STATUS_NOTIFY).includes(value),
  },
  message: {
    type: String,
    default: '',
  },
});

const emit = defineEmits(["update:modelValue", "validate"]);

// =======================
// Computed Region
// =======================
/**
 * Computed property để quản lý giá trị ngày tháng cho BaseDatePicker component
 *
 * @property {Object} value - Giá trị ngày tháng đã được xử lý
 *
 * @function get
 * @description Lấy giá trị modelValue từ props và chuyển đổi sang định dạng Day.js
 * @returns {dayjs.Dayjs|null}
 *
 * @function set
 * @description Cập nhật giá trị modelValue thông qua emit event khi người dùng chọn ngày
 * @param {dayjs.Dayjs|null} date -
 * @emits update:modelValue
 *
 */
const value = computed({
  get() {
    return props.modelValue ? dayjs(props.modelValue) : null;
  },
  set(date) {
    emit("update:modelValue", date ? date.toDate() : null);
    emit("validate");
  },
});
</script>
<template>
  <div class="base-datepicker">
    <!-- Label -->
    <div class="base-datepicker__label">
      <span class="base-datepicker__label-left">
        {{ label }}
        <span v-if="required" class="base-datepicker__required">*</span>
      </span>
      <span class="base-datepicker__label-right">
        <slot name="label-right"></slot>
      </span>
    </div>
    <!-- Content -->
    <div
      class="base-datepicker__content"
      :class="{
        'base-datepicker__content--success': status === STATUS_NOTIFY.SUCCESS,
        'base-datepicker__content--error': status === STATUS_NOTIFY.ERROR,
      }"
    >
      <a-date-picker
        :disabled="disable"
        class="base-datepicker__control"
        format="DD/MM/YYYY"
        :placeholder="placeholder"
        v-model:value="value"
        v-bind="attrs"
      />
        <i
          v-if="status === STATUS_NOTIFY.SUCCESS"
          class="ms-input__icon-status ms-input__icon-status--success fa fa-check-circle"
        ></i>
        <i
          v-if="status === STATUS_NOTIFY.ERROR"
          class="ms-input__icon-status ms-input__icon-status--error fa fa-exclamation-circle"
        ></i>
    </div>
    <!-- Message -->
    <div
      v-if="message"
      class="ms-input__message"
      :class="{
        'ms-input__message--success': status === STATUS_NOTIFY.SUCCESS,
        'ms-input__message--error': status === STATUS_NOTIFY.ERROR,
      }"
    >
      {{ message }}
    </div>
  </div>
    
</template>
<style scoped>
.base-datepicker {
  display: flex;
  flex: 1;
  min-width: 0;
  flex-direction: column;
  gap: 6px;
}

/* Label */
.base-datepicker__label {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-primary);
}

/* Required star */
.base-datepicker__required {
  color: #ef4444;
  margin-left: 4px;
}

.base-datepicker__label-right {
  font-size: 14px;
  color: #b2b2b2;
  white-space: nowrap;
}

/* Content */
.base-datepicker__content {
  display: flex;
  align-items: center;
  height: 36px;
  width: 100%;
  box-sizing: border-box;
  border: 1px solid #e5e7eb;
  border-radius: 4px;
  overflow: hidden;
}

.base-datepicker__content:hover,
.base-datepicker__content:focus-within {
  border-color: var(--color-branch-primary) !important;
  box-shadow: none !important;
}

.base-datepicker__content--success {
  border-color: #10b981 !important;
  border-width: 1.5px !important;
}

.base-datepicker__content--error {
  border-color: #ef4444 !important;
  border-width: 1.5px !important;
}

.base-datepicker__control {
  height: 100%;
  width: 100%;
  flex: 1;
  min-width: 0;
}

:deep(.base-datepicker__content .ant-picker) {
  border: none !important;
  box-shadow: none !important;
  background: transparent !important;
  color: var(--color-text-placeholder) !important;
}

:deep(.base-datepicker__control .ant-picker-input input::placeholder) {
  color: var(--color-text-placeholder) !important;
  font-weight: 300;
  font-family: 'Inter', system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  font-size: 14px;
  opacity: 1;
}

</style>