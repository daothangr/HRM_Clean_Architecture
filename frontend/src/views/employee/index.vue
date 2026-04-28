<script setup>
// =========================
// Imports
// =========================
import BaseButton from "@/components/base/BaseButton.vue"
import BaseInput from "@/components/base/BaseInput.vue"
import BaseTable from "@/components/base/BaseTable.vue"
import { getEmployeeById, getEmployees, createEmployee, updateEmployee, deleteEmployee } from "@/api/employee"
import { formatDate } from "@/utils/formatters"
import EmployeeForm from "./EmployeeForm.vue"
import { computed, onMounted, ref } from "vue"
import { GENDER_OPTIONS, STATUS_EMPLOYEE_OPTIONS } from "../../constants/option"
import { EMPLOYEE_STATUS } from "../../constants/enum"
import { useToast } from '@/composables/useToast'
import { useAuthStore } from "@/stores/auth"
import { scrollToTop } from '@/utils/scroll'

// =========================
// Stores, state and constants
// =========================
const authStore = useAuthStore()
const searchKeyword = ref("")
const employeeData = ref([])
const isTableLoading = ref(false)
const isShowEmployeeForm = ref(false)
const selectedEmployee = ref(null)
const currentPage = ref(1)
const pageSize = ref(10)
const totalEmployees = ref(0)
const toast = useToast()

// Table column definitions
const employeeColumns = [
  { title: "Mã nhân viên", dataIndex: "employeeCode", key: "employeeCode" },
  { title: "Họ và tên", dataIndex: "fullName", key: "fullName" },
  { title: "Email", dataIndex: "email", key: "email" },
  { title: "Giới tính", dataIndex: "gender", key: "gender" },
  { title: "Ngày sinh", dataIndex: "dateOfBirth", key: "dateOfBirth" },
  { title: "Địa chỉ", dataIndex: "address", key: "address" },
  { title: "Điện thoại", dataIndex: "phone", key: "phone" },
  { title: "Ngày vào làm", dataIndex: "hireDate", key: "hireDate" },
  { title: "Phòng ban", dataIndex: "departmentName", key: "departmentName" },
  { title: "Vị trí", dataIndex: "position", key: "position" },
  { title: "Trạng thái", dataIndex: "status", key: "status" },
  { title: "", key: "actions", dataIndex: "actions", width: 90 }
]

const EMPLOYEE_STATUS_CLASS_MAP = Object.freeze({
  [EMPLOYEE_STATUS.Active]: 'employee-status-badge--active',
  [EMPLOYEE_STATUS.Probation]: 'employee-status-badge--probation',
  [EMPLOYEE_STATUS.Resigned]: 'employee-status-badge--resigned',
})


// =========================
// UI actions
// =========================
const handleOpenFormAddEmployee = () => { 
  scrollToTop()
  selectedEmployee.value = null
  isShowEmployeeForm.value = true
}

const handleCloseEmployeeForm = () => {
  isShowEmployeeForm.value = false
  selectedEmployee.value = null
}

// =========================
// Table configs
// =========================


/**
 * Tạo cấu hình phân trang cho bảng
 * Dựa trên currentPage, pageSize và totalEmployees để tạo đối tượng cấu hình phân trang phù hợp với Ant Design Vue
 */
const tablePagination = computed(() => ({
  current: currentPage.value,
  pageSize: pageSize.value,
  total: totalEmployees.value,
  showSizeChanger: true,
  position: ['bottomRight'],
  pageSizeOptions: ['10', '20', '50'],
  showTotal: (total, range) => `${range[0]}-${range[1]} / ${total}`
}))


// =========================
// API actions
// =========================
/**
 * Hàm lấy danh sách nhân viên từ API
 * Và cập nhật state liên quan đến danh sách nhân viên, phân trang và trạng thái tải
 * @returns {Promise<void>} Promise xử lý việc lấy dữ liệu và cập nhật state.
 */
const fetchEmployees = async () => {
  isTableLoading.value = true

  try {
    const response = await getEmployees({
      pageNumber: currentPage.value,
      pageSize: pageSize.value
    })
    const paginationData = response?.data ?? {}
    employeeData.value = paginationData.items ?? []
    totalEmployees.value = paginationData.totalCount ?? employeeData.value.length
    currentPage.value = paginationData.pageNumber ?? currentPage.value
    pageSize.value = paginationData.pageSize ?? pageSize.value
    console.log('Fetched employees:', employeeData.value)
  } catch (error) {
    console.error('Failed to fetch employees:', error)
    employeeData.value = []
    totalEmployees.value = 0
  } finally {
    isTableLoading.value = false
  }
}

/**
 * Hàm xử lý khi bảng thay đổi (phân trang, lọc, sắp xếp)
 * @param {Object} pagination - Thông tin phân trang mới
 * @returns {Promise<void>} Promise xử lý việc cập nhật phân trang và lấy lại dữ liệu.
 */
const handleTableChange = async (pagination) => {
  const nextPageSize = pagination?.pageSize ?? pageSize.value
  const nextPage = pagination?.current ?? currentPage.value
  const isPageSizeChanged = nextPageSize !== pageSize.value

  pageSize.value = nextPageSize
  currentPage.value = isPageSizeChanged ? 1 : nextPage

  await fetchEmployees()
}


/**
 * Hàm xử lý khi nhấn nút chỉnh sửa nhân viên - mở form với dữ liệu nhân viên đã chọn
 * @param {Object} employee - Dữ liệu nhân viên được chọn để chỉnh sửa
 * @returns {Promise<void>} Promise xử lý việc lấy chi tiết nhân viên và mở form
 */
const handleEditEmployee = async (employee) => {
  if (!employee?.id) {
    return
  }

  try {
    const response = await getEmployeeById(employee.id)
    selectedEmployee.value = response.data
    scrollToTop()
    isShowEmployeeForm.value = true
    console.log('Selected employee for edit:', selectedEmployee.value)
  } catch (error) {
    console.error('Failed to fetch employee detail:', error)
  }
}

/**
 * Hàm xử lý khi form submit - tạo hoặc cập nhật nhân viên
 *
 * @param {Object} formData - Dữ liệu từ form
 * @returns {Promise<void>}
 */
const handleSubmitEmployeeForm = async (formData) => {
  try {
    if (formData.id) {
      // Cập nhật nhân viên hiện có
      await updateEmployee(formData.id, formData)
      console.log('Employee updated successfully')
      toast.success('Cập nhật nhân viên thành công')
    } else {
      // Tạo nhân viên mới
      await createEmployee(formData)
      console.log('Employee created successfully')
      toast.success('Tạo nhân viên thành công')
    }
    // Đóng form và load lại danh sách
    handleCloseEmployeeForm()
    await fetchEmployees()
  } catch (error) {
    console.error('Failed to save employee:', error)
    toast.error('Có lỗi xảy ra khi lưu nhân viên')
  }
}

/**
 * Hàm xử lý khi xóa nhân viên
 * @param {Object} employee - Dữ liệu nhân viên được chọn để xóa
 * @returns {Promise<void>}
 */
const handleDeleteEmployee = async (employee) => {
  if (!employee?.id) {
    return
  }

  if (!window.confirm(`Bạn có chắc chắn muốn xóa nhân viên ${employee.fullName}?`)) {
    return
  }

  try {
    await deleteEmployee(employee.id)
    console.log('Employee deleted successfully')
    toast.success('Xóa nhân viên thành công')
    await fetchEmployees()
  } catch (error) {
    console.error('Failed to delete employee:', error)
    toast.error('Có lỗi xảy ra khi xóa nhân viên')
  }
}



// =========================
// Lifecycle
// =========================
onMounted(() => {
  fetchEmployees()
})

</script>
<template>
  <section class="employee-page">
    <!-- Title -->
    <div class="main-content__title">
      <div>
        <h1 class="title-name">Nhân viên</h1>
        <p class="employee-page__subtitle">Danh sách nhân viên toàn công ty</p>
      </div>

      <BaseButton iconClass="fas fa-plus" @click="handleOpenFormAddEmployee">
        Thêm nhân viên
      </BaseButton>
    </div>

    <div class="main-content__wrap">
      <div class="toolbar">
        <div class="toolbar-search">
          <BaseInput
            v-model="searchKeyword"
            placeholder="Tìm kiếm nhân viên"
            iconClass="fa-solid fa-magnifying-glass"
          />
        </div>

        <!-- Toolbar actions -->
        <div class="toolbar-actions">
          <button class="toolbar-icon" type="button" aria-label="Lọc">
            <i class="fa-solid fa-filter"></i>
          </button>
          <button class="toolbar-icon" type="button" aria-label="Sắp xếp">
            <i class="fa-solid fa-arrow-down-wide-short"></i>
          </button>
          <button class="toolbar-icon" type="button" aria-label="Cài đặt">
            <i class="fa-solid fa-gear"></i>
          </button>
        </div>
      </div>
      <!-- Employee Table -->
      <base-table
        class="employee-table"
        :columns="employeeColumns"
        :data="employeeData"
        :loading="isTableLoading"
        :pagination="tablePagination"
        row-key="id"
        :scroll = "{ y: 'calc(100vh - 390px)'}"
        @change="handleTableChange"
      >
        <template #bodyCell="{ column, text, record }">
          <template v-if="column.dataIndex === 'status'">
            <span class="employee-status-badge" :class="EMPLOYEE_STATUS_CLASS_MAP[Number(text)] || 'employee-status-badge--default'">
              {{ STATUS_EMPLOYEE_OPTIONS.find(option => option.value === text)?.label || '--' }}
            </span>
          </template>
          <template v-else-if="column.dataIndex === 'gender'">
            {{ GENDER_OPTIONS.find(option => option.value === text)?.label || '--' }}
          </template>
          <template v-else-if="column.dataIndex === 'dateOfBirth'">
            {{ text ? formatDate(text) : '--' }}
          </template>
          <template v-else-if="column.dataIndex === 'hireDate'">
            {{ text ? formatDate(text) : '--' }}
          </template>
          <template v-else-if="column.key === 'actions'">
            <div class="row-action-buttons">
              <button
                v-permission="['Admin']"
                class="row-action-button"
                type="button"
                aria-label="Chỉnh sửa nhân viên"
                @click="handleEditEmployee(record)"
              >
                <i class="fa-solid fa-pen-to-square"></i>
              </button>
              <button
                v-permission="['Admin']"
                class="row-action-button row-action-button--delete"
                type="button"
                aria-label="Xóa nhân viên"
                @click="handleDeleteEmployee(record)"
              >
                <i class="fa-solid fa-trash"></i>
              </button>
            </div>
          </template>
          <template v-else>
            {{ text || '--' }}
          </template>
        </template>
      </base-table>
    </div>

    <EmployeeForm
      v-if="isShowEmployeeForm"
      :initialData="selectedEmployee"
      @close="handleCloseEmployeeForm"
      @submit="handleSubmitEmployeeForm"
    />
  </section>
</template>
<style scoped>
.employee-page {
  display: grid;
  gap: 16px;
}

.employee-page__subtitle {
  margin: 4px 0 0;
  color: #64748b;
  font-size: 14px;
}

.toolbar-icon:hover {
  border-color: #4387ee;
  color: #4387ee;
  background: #f1f7ff;
}

.employee-page :deep(.employee-table .row-action-button--delete) {
  color: #ef4444;
}

.employee-page :deep(.employee-table .row-action-button.row-action-button--delete:hover) {
  background: #fee2e2;
  border-color: #fca5a5;
  color: #ef4444;
}

.employee-page :deep(.employee-table .ant-table-wrapper) {
  height: 560px;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 14px 30px rgba(15, 23, 42, 0.1);
}

.employee-page :deep(.employee-table .ant-spin-nested-loading),
.employee-page :deep(.employee-table .ant-spin-container) {
  height: 100%;
}

.employee-page :deep(.employee-table .ant-table) {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.employee-page :deep(.employee-table .ant-table-container) {
  flex: 1;
  min-height: 0;
  border: 1px solid #e8edf5;
}

.employee-page :deep(.employee-table .ant-pagination) {
  position: sticky;
  bottom: 0;
  z-index: 2;
  margin: 0;
  padding: 12px 16px;
  background: #ffffff;
  border-top: 1px solid #e8edf5;
}

.employee-page :deep(.employee-table .ant-table-cell:last-child) {
  text-align: center;
}

.employee-status-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 120px;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
  line-height: 1.4;
}

.employee-status-badge--active {
  color: #065f46;
  background: #d1fae5;
}

.employee-status-badge--probation {
  color: #1d4ed8;
  background: #dbeafe;
}

.employee-status-badge--resigned {
  color: #9a3412;
  background: #ffedd5;
}

.employee-status-badge--default {
  color: #334155;
  background: #e2e8f0;
}

@media (max-width: 992px) {
}
</style>