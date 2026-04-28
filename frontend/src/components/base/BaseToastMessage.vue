<script setup>
import { useToast } from '@/composables/useToast'

const { toasts, remove } = useToast()

const toastIcon = {
  success: '✓',
  error: '!',
  warning: '!',
}
</script>

<template>
  <div class="toast-container">
    <TransitionGroup name="toast">
      <div
        v-for="toast in toasts"
        :key="toast.id"
        :class="['toast', toast.type]"
        @click="remove(toast.id)"
      >
        <span class="toast-icon">{{ toastIcon[toast.type] || 'i' }}</span>
        <p class="toast-message">{{ toast.message }}</p>
        <button
          type="button"
          class="toast-close"
          @click.stop="remove(toast.id)"
        >
          ×
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>

<style scoped>
.toast-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 10px;
  width: min(92vw, 360px);
}

.toast {
  display: grid;
  grid-template-columns: 28px 1fr 28px;
  align-items: center;
  gap: 10px;
  padding: 12px;
  border-radius: 12px;
  border: 1px solid transparent;
  color: #122230;
  box-shadow: 0 14px 32px rgba(0, 0, 0, 0.18);
  backdrop-filter: blur(4px);
  cursor: pointer;
}

.toast-icon {
  width: 28px;
  height: 28px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 999px;
  font-weight: 800;
}

.toast-message {
  margin: 0;
  line-height: 1.35;
  font-size: 14px;
}

.toast-close {
  width: 28px;
  height: 28px;
  border: none;
  border-radius: 999px;
  font-size: 18px;
  line-height: 1;
  background: rgba(18, 34, 48, 0.12);
  color: #122230;
  cursor: pointer;
}

.toast-close:hover {
  background: rgba(18, 34, 48, 0.2);
}

.success {
  background: linear-gradient(135deg, #e6fff1 0%, #d3f7e6 100%);
  border-color: #68d391;
}

.success .toast-icon {
  background: #1f9d55;
  color: #ffffff;
}

.error {
  background: linear-gradient(135deg, #ffeef0 0%, #ffd7dc 100%);
  border-color: #f87171;
}

.error .toast-icon {
  background: #dc2626;
  color: #ffffff;
}

.warning {
  background: linear-gradient(135deg, #fff8e1 0%, #ffe8b5 100%);
  border-color: #f59e0b;
}

.warning .toast-icon {
  background: #d97706;
  color: #ffffff;
}

.toast-enter-active,
.toast-leave-active {
  transition: all 0.25s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateX(24px) scale(0.96);
}

.toast-move {
  transition: transform 0.25s ease;
}

@media (max-width: 576px) {
  .toast-container {
    top: 12px;
    left: 12px;
    right: 12px;
    width: auto;
  }
}
</style>