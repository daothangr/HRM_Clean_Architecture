<script setup>
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseDatePicker from '@/components/base/BaseDatePicker.vue'
import BaseDropBox from '@/components/base/BaseDropBox.vue'
import { GENDER_OPTIONS, STATUS_EMPLOYEE_OPTIONS, ROLE_OPTIONS } from '../../constants/option'
import { STATUS_NOTIFY } from '../../constants/enum'
import { computed, reactive, watch } from 'vue'
import { useFormState } from '@/composables/useFormStatus'
import { isEmptyValue } from '@/utils/validation'


const props = defineProps({
  initialData: {
    type: Object,
    default: null,
  },
})

const emit = defineEmits(['close', 'submit'])

const isEditMode = computed(() => Boolean(props.initialData?.id))


/**
 * Trạng thái mặc định của form nhân viên.
 * @returns {Object}
 */
const getDefaultFormData = () => ({
  id: '',
  employeeCode: '',
  password: '',
  newPassword: '',
  fullName: '',
  gender: '',
  phone: '',
  email: '',
  departmentId: '',
  managerId: null,
  dateOfBirth: null,
  address: '',
  hireDate: null,
  resignDate: null,
  status: '',
  roleName: 'Employee',
})

const formData = reactive(getDefaultFormData())

const { formState, setFieldState, clearFieldState, clearAllStates } = useFormState()


/**
 * Đóng form và phát sự kiện close tới parent component.
 * @returns {void}
 */
const handleClose = () => {
  emit('close')
}

/**
 * Hàm validate form trước khi submit. Kiểm tra các trường bắt buộc và cập nhật trạng thái của từng trường.
 * @returns {boolean} true when valid, false otherwise
 */
const validateForm = () => {
  // reset previous field states
  clearAllStates()

  const requiredFields = ['fullName', 'email', 'departmentId', 'roleName', 'hireDate']
  if (!isEditMode.value) {
    requiredFields.unshift('employeeCode', 'password')
  } else {
    requiredFields.push('status')
  }

  const errors = []

  requiredFields.forEach((field) => {
    const value = formData[field]

    if (isEmptyValue(value)) {
      setFieldState(field, STATUS_NOTIFY.ERROR, 'Trường này là bắt buộc')
      errors.push(field)
    } else {
      setFieldState(field, STATUS_NOTIFY.SUCCESS, '')
    }
  })

  return errors.length === 0
}


/**
 * Xử lý submit form và phát sự kiện submit tới parent component cùng dữ liệu form.
 * @returns {void}
 */
const handleSubmit = () => {
  const normalizeRoleName = (value) => {
    if (Array.isArray(value)) {
      return String(value[0] ?? '').trim()
    }
    return String(value ?? '').trim()
  }

  if (!validateForm()) {
    return
  }

  const payload = {
    ...formData,
    roleName: normalizeRoleName(formData.roleName),
  }

  // emit submit when all required fields are valid
  emit('submit', payload)
  console.log('Form submitted with data:', payload)
}

/**
 * Điền dữ liệu nhân viên vào form. Nếu không có dữ liệu thì reset form về giá trị mặc định.
 *
 * @param {Object|null} employee - Dữ liệu nhân viên từ API
 * @returns {void}
 */

const fillFormData = (employee) => {
  if (!employee) {
    Object.assign(formData, getDefaultFormData())
    return
  }

  const normalizedRoleName = Array.isArray(employee.roleName)
    ? employee.roleName[0]
    : Array.isArray(employee.roles)
      ? employee.roles[0]
      : employee.roleName

  Object.assign(formData, {
    ...getDefaultFormData(),
    ...employee,
    roleName: normalizedRoleName ?? 'Employee',
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
  <div class="form-overlay employee-form-overlay">
    <div class="form-modal employee-form-modal">
      <div class="form-header employee-form-header">
        <div class="form-title">{{ isEditMode ? 'Sửa nhân viên' : 'Thêm nhân viên' }}</div>
        <button class="close-button" type="button" aria-label="Đóng form" @click="handleClose">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </div>

      <form class="form-body employee-form-body" @submit.prevent="handleSubmit">
        <div class="form-sections-wrapper employee-form-sections-wrapper">
          <div class="form-sections">
          <section class="form-section">
            <h3 class="form-section-title">Thông tin xác thực</h3>
            <div class="form-grid">
              <BaseInput v-if="!isEditMode" v-model="formData.employeeCode" placeholder="Nhập mã nhân viên" required
                :status="formState.employeeCode?.status" :message="(formState.employeeCode?.message || '')">
                Mã nhân viên
              </BaseInput>

              <BaseInput
                v-if="!isEditMode"
                v-model="formData.password"
                placeholder="Nhập mật khẩu"
                type="password"
                required
                :status="formState.password?.status" :message="(formState.password?.message || '')"
              >
                Mật khẩu
              </BaseInput>

              <BaseInput
                v-else
                v-model="formData.newPassword"
                placeholder="Nhập mật khẩu mới"
                type="password"
                :status="formState.newPassword?.status" :message="(formState.newPassword?.message || '')"
              >
                Mật khẩu mới
              </BaseInput>
            </div>
          </section>

          <section class="form-section">
            <h3 class="form-section-title">Thông tin cá nhân</h3>
            <div class="form-grid form-grid--two-columns">
              <BaseInput v-model="formData.fullName" placeholder="Nhập họ và tên" required
                :status="formState.fullName?.status" :message="(formState.fullName?.message || '')">
                Họ và tên
              </BaseInput>

              <BaseDropBox
                v-model="formData.gender"
                placeholder="Chọn giới tính"
                :options="GENDER_OPTIONS"
              >
                Giới tính
              </BaseDropBox>

              <BaseDatePicker
                v-if="isEditMode"
                v-model="formData.dateOfBirth"
                placeholder="Chọn ngày sinh"
                label="Ngày sinh"
              />

              <BaseInput v-if="isEditMode" v-model="formData.address" placeholder="Nhập địa chỉ">
                Địa chỉ
              </BaseInput>

              <BaseInput v-model="formData.phone" placeholder="Nhập số điện thoại">
                Số điện thoại
              </BaseInput>

              <BaseInput v-model="formData.email" placeholder="Nhập email" required
                :status="formState.email?.status" :message="(formState.email?.message || '')">
                Email
              </BaseInput>
            </div>
          </section>

          <section class="form-section">
            <h3 class="form-section-title">Công việc</h3>
            <div class="form-grid form-grid--two-columns">
              <BaseDropBox v-model="formData.departmentId" placeholder="Chọn phòng ban" route="/departments" required
                :status="formState.departmentId?.status" :message="(formState.departmentId?.message || '')">
                Phòng ban
              </BaseDropBox>

              <BaseDropBox v-model="formData.position" placeholder="Chọn vị trí công việc">
                Vị trí công việc
              </BaseDropbox>
              
              <BaseDropBox v-model="formData.managerId" placeholder="Chọn quản lý" route="/employees/managers">
                Quản lý
              </BaseDropBox>

              <BaseDropBox v-model="formData.roleName" placeholder="Chọn Role" :options="ROLE_OPTIONS" required
                :status="formState.roleName?.status" :message="(formState.roleName?.message || '')">
                Vai trò
              </BaseDropBox>

              <BaseDatePicker
                v-model="formData.hireDate"
                placeholder="Chọn ngày vào làm"
                required
                label="Ngày vào làm"
                :status="formState.hireDate?.status" :message="(formState.hireDate?.message || '')"
              />

              <BaseDropBox
                v-if="isEditMode"
                v-model="formData.status"
                placeholder="Chọn trạng thái"
                :options="STATUS_EMPLOYEE_OPTIONS"
                required
                :status="formState.status?.status" :message="(formState.status?.message || '')"
              >
                Trạng thái
              </BaseDropBox>

              <BaseDatePicker
                v-if="isEditMode"
                v-model="formData.resignDate"
                placeholder="Chọn ngày nghỉ việc"
                label="Ngày nghỉ việc"
                :status="formState.resignDate?.status" :message="(formState.resignDate?.message || []).join(', ')"
              />
            </div>
          </section>
        </div>
        </div>

        <div class="form-actions employee-form-actions">
          <BaseButton backgroundColor="#eef2f7" textColor="#334155" @click="handleClose">
            Hủy
          </BaseButton>
          <BaseButton iconClass="fa-solid fa-floppy-disk" @click="handleSubmit">
            {{ isEditMode ? 'Cập nhật nhân viên' : 'Lưu nhân viên' }}
          </BaseButton>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.employee-form-overlay {
  --form-overlay-padding: 24px;
}

.employee-form-modal {
  --form-modal-width: 960px;
}

.employee-form-header {
  --form-header-padding: 25px 28px;
}

.close-button {
  width: 36px;
  height: 36px;
  border: none;
}

.employee-form-sections-wrapper {
  --form-sections-wrapper-padding: 12px 24px;
}

@media (max-width: 768px) {
  .employee-form-overlay {
    --form-overlay-padding: 12px;
  }
}
</style>
