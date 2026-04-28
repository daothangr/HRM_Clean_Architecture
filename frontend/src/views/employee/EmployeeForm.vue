<script setup>
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseDatePicker from '@/components/base/BaseDatePicker.vue'
import BaseDropBox from '@/components/base/BaseDropBox.vue'
import { GENDER_OPTIONS, STATUS_EMPLOYEE_OPTIONS, ROLE_OPTIONS } from '../../constants/option'
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
  const normalizeRoleName = (value) => {
    if (Array.isArray(value)) {
      return String(value[0] ?? '').trim()
    }
    return String(value ?? '').trim()
  }

  const payload = {
    ...formData,
    roleName: normalizeRoleName(formData.roleName),
  }

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
              <BaseInput v-if="!isEditMode" v-model="formData.employeeCode" placeholder="Nhập mã nhân viên" required>
                Mã nhân viên
              </BaseInput>

              <BaseInput
                v-if="!isEditMode"
                v-model="formData.password"
                placeholder="Nhập mật khẩu"
                type="password"
                required
              >
                Mật khẩu
              </BaseInput>

              <BaseInput
                v-else
                v-model="formData.newPassword"
                placeholder="Nhập mật khẩu mới"
                type="password"
              >
                Mật khẩu mới
              </BaseInput>
            </div>
          </section>

          <section class="form-section">
            <h3 class="form-section-title">Thông tin cá nhân</h3>
            <div class="form-grid form-grid--two-columns">
              <BaseInput v-model="formData.fullName" placeholder="Nhập họ và tên" required>
                Họ và tên
              </BaseInput>

              <BaseDropBox
                v-model="formData.gender"
                placeholder="Chọn giới tính"
                :options="GENDER_OPTIONS"
                required
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

              <BaseInput v-model="formData.email" placeholder="Nhập email" required>
                Email
              </BaseInput>
            </div>
          </section>

          <section class="form-section">
            <h3 class="form-section-title">Công việc</h3>
            <div class="form-grid form-grid--two-columns">
              <BaseDropBox v-model="formData.departmentId" placeholder="Chọn phòng ban" route="/departments" required>
                Phòng ban
              </BaseDropBox>

              <BaseDropBox v-model="formData.position" placeholder="Chọn vị trí công việc">
                Vị trí công việc
              </BaseDropbox>
              
              <BaseDropBox v-model="formData.managerId" placeholder="Chọn quản lý" route="/employees/managers">
                Quản lý
              </BaseDropBox>

              <BaseDropBox v-model="formData.roleName" placeholder="Chọn Role" :options="ROLE_OPTIONS" required>
                Vai trò
              </BaseDropBox>

              <BaseDatePicker
                v-model="formData.hireDate"
                placeholder="Chọn ngày vào làm"
                required
                label="Ngày vào làm"
              />

              <BaseDropBox
                v-if="isEditMode"
                v-model="formData.status"
                placeholder="Chọn trạng thái"
                :options="STATUS_EMPLOYEE_OPTIONS"
                required
              >
                Trạng thái
              </BaseDropBox>

              <BaseDatePicker
                v-if="isEditMode"
                v-model="formData.resignDate"
                placeholder="Chọn ngày nghỉ việc"
                label="Ngày nghỉ việc"
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
