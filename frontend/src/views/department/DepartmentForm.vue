<script setup>
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseDropBox from '@/components/base/BaseDropBox.vue'
import { computed, reactive, watch } from 'vue'


const props = defineProps({
	initialData: {
		type: Object,
		default: null,
	},
})

const emit = defineEmits(['close', 'submit'])

const isEditMode = computed(() => Boolean(props.initialData?.id))


/**
 * Trạng thái mặc định của form phòng ban.
 * @returns {Object}
 */
const getDefaultFormData = () => ({
	id: '',
	code: '',
	name: '',
	parentDepartmentId: null,
	departmentHeadId: null,
})

const formData = reactive(getDefaultFormData())


/**
 * Đóng form và phát sự kiện close tới parent component.
 * @returns {void}
 */
const handleClose = () => {
	emit('close')
}

/**
 * Xử lý submit form và phát sự kiện submit tới parent component cùng dữ liệu form.
 * @returns {void}
 */
const handleSubmit = () => {
	const payload = {
		...formData,
		code: String(formData.code ?? '').trim(),
		name: String(formData.name ?? '').trim(),
	}

	emit('submit', payload)
	console.log('Department form submitted with data:', payload)
}

/**
 * Điền dữ liệu phòng ban vào form. Nếu không có dữ liệu thì reset form về giá trị mặc định.
 *
 * @param {Object|null} department - Dữ liệu phòng ban từ API
 * @returns {void}
 */
const fillFormData = (department) => {
	if (!department) {
		Object.assign(formData, getDefaultFormData())
		return
	}

	Object.assign(formData, {
		...getDefaultFormData(),
		...department,
	})
}

/**
 * Theo dõi sự thay đổi của dữ liệu ban đầu và cập nhật form accordingly.
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
	<div class="department-form-overlay">
		<div class="department-form-modal">
			<div class="department-form-header">
				<div class="department-form-title">{{ isEditMode ? 'Sửa phòng ban' : 'Thêm phòng ban' }}</div>
				<button class="close-button" type="button" aria-label="Đóng form" @click="handleClose">
					<i class="fa-solid fa-xmark"></i>
				</button>
			</div>

			<form class="department-form-body" @submit.prevent="handleSubmit">
				<div class="form-sections-wrapper">
					<div class="form-sections">
						<section class="form-section">
							<h3 class="form-section-title">Thông tin phòng ban</h3>
							<div class="form-grid form-grid--two-columns">
								<BaseInput v-model="formData.code" placeholder="Nhập mã phòng ban" required>
									Mã phòng ban
								</BaseInput>

								<BaseInput v-model="formData.name" placeholder="Nhập tên phòng ban" required>
									Tên phòng ban
								</BaseInput>

								<BaseDropBox
									v-model="formData.parentDepartmentId"
									placeholder="Chọn phòng ban cha"
									route="/departments"
								>
									Phòng ban cha
								</BaseDropBox>

								<BaseDropBox
									v-model="formData.departmentHeadId"
									placeholder="Chọn trưởng phòng"
									route="/employees/managers"
								>
									Trưởng phòng
								</BaseDropBox>
							</div>
						</section>
					</div>
				</div>

				<div class="form-actions">
					<BaseButton backgroundColor="#eef2f7" textColor="#334155" @click="handleClose">
						Hủy
					</BaseButton>
					<BaseButton iconClass="fa-solid fa-floppy-disk" @click="handleSubmit">
						{{ isEditMode ? 'Cập nhật phòng ban' : 'Lưu phòng ban' }}
					</BaseButton>
				</div>
			</form>
		</div>
	</div>
</template>

<style scoped>
.department-form-overlay {
	position: fixed;
	inset: 0;
	z-index: 1000;
	display: grid;
	place-items: center;
	background: rgba(15, 23, 42, 0.45);
	padding: 24px;
}

.department-form-modal {
	display: flex;
	flex-direction: column;
	width: min(960px, 100%);
	max-height: 80vh;
	border-radius: 16px;
	background: #ffffff;
	box-shadow: 0 24px 60px rgba(15, 23, 42, 0.22);
}

.department-form-header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
	padding: 25px 28px;
	flex-shrink: 0;
}

.department-form-title {
	margin: 0;
	font-size: 25px;
	color: #0f172a;
	font-weight: 700;
}

.close-button {
	width: 36px;
	height: 36px;
	border: none;
	border-radius: 10px;
	background: #f8fafc;
	color: #475569;
	cursor: pointer;
}

.department-form-body {
	flex: 1;
	overflow-y: auto;
	display: flex;
	flex-direction: column;
}

.form-sections-wrapper {
    flex: 1;
    overflow-y: auto;
    padding: 12px 24px 26px;
}

.form-sections {
	display: flex;
	flex-direction: column;
	gap: 10px;
}

.form-section {
	border-radius: 12px;
	padding: 10px 16px;
}

.form-section-title {
	margin: 0 0 12px;
	font-size: 18px;
	font-weight: 600;
	color: #1e293b;
}

.form-grid {
	display: grid;
	grid-template-columns: 1fr;
	gap: 14px;
}

.form-grid--two-columns {
	grid-template-columns: repeat(2, minmax(0, 1fr));
}

@media (max-width: 1024px) {
	.form-grid--two-columns {
		grid-template-columns: 1fr;
	}
}

.form-actions {
	display: flex;
	justify-content: flex-end;
	gap: 12px;
	padding: 20px 28px;
	flex-shrink: 0;
	border-top: 1px solid #e2e8f0;
	background: #f8fafc;
	border-radius: 0 0 16px 16px;
}

@media (max-width: 768px) {
	.department-form-overlay {
		padding: 12px;
	}

	.department-form-header,
	.department-form-body {
		padding: 16px;
	}

	.form-actions {
		flex-direction: column-reverse;
	}
}
</style>
