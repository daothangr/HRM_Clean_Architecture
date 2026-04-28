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
  <div class="employee-form-overlay">
    <div class="employee-form-modal">
      <div class="employee-form-header">
        <div class="employee-form-title">{{ isEditMode ? 'Sửa nhân viên' : 'Thêm nhân viên' }}</div>
        <button class="close-button" type="button" aria-label="Đóng form" @click="handleClose">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </div>

      <form class="employee-form-body" @submit.prevent="handleSubmit">
        <div class="form-sections-wrapper">
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

        <div class="form-actions">
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
  position: fixed;
  inset: 0;
  z-index: 1000;
  display: grid;
  place-items: center;
  background: rgba(15, 23, 42, 0.45);
  padding: 24px;
}

.employee-form-modal {
  display: flex;
  flex-direction: column;
  width: min(960px, 100%);
  max-height: 80vh;
  border-radius: 16px;
  background: #ffffff;
  box-shadow: 0 24px 60px rgba(15, 23, 42, 0.22);
}

.employee-form-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 25px 28px;
  flex-shrink: 0;
}

.employee-form-title {
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

.employee-form-body {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

.form-sections-wrapper {
  flex: 1;
  overflow-y: auto;
  padding: 12px 24px;
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
  .employee-form-overlay {
    padding: 12px;
  }

  .employee-form-header,
  .employee-form-body {
    padding: 16px;
  }

  .form-actions {
    flex-direction: column-reverse;
  }
}
</style>
