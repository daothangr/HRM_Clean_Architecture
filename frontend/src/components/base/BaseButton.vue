<script setup>
import { computed } from 'vue'

const props = defineProps({
	iconClass: {
		type: String,
		default: ''
	},
	textColor: {
		type: String,
		default: '#ffffff'
	},
	backgroundColor: {
		type: String,
		default: 'var(--color-branch-primary)'
	},
	disabled: {
		type: Boolean,
		default: false
	}
})

const emit = defineEmits(['click'])

const buttonStyle = computed(() => ({
	color: props.textColor,
	backgroundColor: props.backgroundColor
}))

const handleClick = (event) => {
	emit('click', event)
}
</script>

<template>
	<button class="base-button" type="button" :style="buttonStyle" :disabled="disabled" @click="handleClick">
		<i v-if="iconClass" :class="iconClass" class="button-icon"></i>
		<span><slot /></span>
	</button>
</template>

<style scoped>
.base-button {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	gap: 8px;
	border: none;
	border-radius: 6px;
	padding: 12px 16px;
	font-size: 14px;
	font-weight: 550;
	line-height: 1;
	cursor: pointer;
	transition: filter 0.2s ease, transform 0.1s ease;
}

.button-icon {
	font-size: 13px;
	display: inline-flex;
	align-items: center;
}

.base-button:hover {
	filter: brightness(0.95);
}

.base-button:active {
	transform: translateY(1px);
}

.base-button:disabled {
	cursor: not-allowed;
	opacity: 0.6;
}
</style>
