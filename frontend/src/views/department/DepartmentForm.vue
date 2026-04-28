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
	<div class="form-overlay">
		<div class="form-modal">
			<div class="form-header">
				<div class="form-title">{{ isEditMode ? 'Sửa phòng ban' : 'Thêm phòng ban' }}</div>
				<button class="close-button" type="button" aria-label="Đóng form" @click="handleClose">
					<i class="fa-solid fa-xmark"></i>
				</button>
			</div>

			<form class="form-body" @submit.prevent="handleSubmit">
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
</style>