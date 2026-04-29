<script setup>
import { useAttrs } from "vue";
import { STATUS_NOTIFY } from "@/constants/enum";
import '@/assets/css/form.css';

defineOptions({ inheritAttrs: false });
const attrs = useAttrs();
// ================
// Props & Emits Region
// ================
const props = defineProps({
  required: {
    type: Boolean,
    default: false,
  },
  modelValue: {
    type: String,
    default: "",
  },
  iconClass: {
    type: String,
    default: "",
  },
  placeholder: {
    type: String,
    default: "",
  },
  disabled: {
    type: Boolean,
    default: false,
  },
  status: {
    type: String,
    default: STATUS_NOTIFY.DEFAULT,
    validator: (value) => Object.values(STATUS_NOTIFY).includes(value),
  },
  message: {
    type: String,
    default: "",
  },
});

const emit = defineEmits(["update:modelValue"]);
</script>

<template>
  <div class="ms-input">
    <!-- Label -->
    <div class="ms-input__label">
      <span class="ms-input__label-left">
        <slot></slot>
        <span v-if="required" class="ms-input__required">*</span>
      </span>
      <span class="ms-input__label-right">
        <slot name="label-right"></slot>
      </span>
    </div>

    <!-- Input wrapper -->
    <div
      class="ms-input__form"
      :class="{
        'ms-input__form--success': status === STATUS_NOTIFY.SUCCESS,
        'ms-input__form--error': status === STATUS_NOTIFY.ERROR,
      }"
    >
      <input
        class="ms-input__control"
        :value="modelValue"
        :placeholder="placeholder"
        :disabled="disabled"
        v-bind="attrs"
        @input="emit('update:modelValue', $event.target.value)"
      />

      <i v-if="iconClass" :class="iconClass" class="ms-input__icon-inside"></i>
      <i
        v-if="status === STATUS_NOTIFY.SUCCESS"
        class="ms-input__icon-inside ms-input__icon-status ms-input__icon-status--success fa fa-check-circle"
      ></i>
      <i
        v-if="status === STATUS_NOTIFY.ERROR"
        class="ms-input__icon-inside ms-input__icon-status ms-input__icon-status--error fa fa-exclamation-circle"
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
/* Wrapper */
.ms-input {
  display: flex;
  flex: 1;
  min-width: 0;
  flex-direction: column;
  gap: 6px;
}

/* Label */
.ms-input__label {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-primary);
}

/* Required star */
.ms-input__required {
  color: #ef4444;
  margin-left: 4px;
}

.ms-input__label-right {
  font-size: 14px;
  color: #b2b2b2;
  white-space: nowrap;
}

/* Input */
.ms-input__form {
  display: flex;
  width: 100%;
  box-sizing: border-box;
  align-items: center;
  border: 1px solid #e5e7eb;
  border-radius: 4px;
  overflow: hidden;
}

.ms-input__form--success {
  border-color: #10b981;
  border-width: 1.5px;
}

.ms-input__form--error{
  border-color: #ef4444;
  border-width: 1.5px;
}

/* Focus */
.ms-input__form:hover,
.ms-input__form:focus-within {
  border-color: var(--color-branch-primary);
  /* box-shadow: 0 0 0 1px rgba(37, 99, 235, 0.2); */
}

/* Error */
.ms-input__form--invalid {
  border-color: var(--color-status-error);
  border-width: 2px;
}

.ms-input__control {
  position: relative;
  height: 36px;
  padding: 0 12px;
  font-size: 14px;
  outline: none;
  transition: all 0.2s ease;
  flex: 1;
  min-width: 0;
  box-sizing: border-box;
  border: none;
}

/* Placeholder */
.ms-input__control::placeholder {
  color: var(--color-text-placeholder);
  font-weight: 200;
}

.ms-input__icon-inside {
  margin-right: 10px;
  cursor: pointer;
  color: #9ca3af;
  padding-left: 6px;
}

</style>