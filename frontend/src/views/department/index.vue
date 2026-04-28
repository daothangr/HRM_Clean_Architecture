<script setup>
// =========================
// Imports
// =========================
import { onMounted, ref } from 'vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseTable from '@/components/base/BaseTable.vue'
import { createDepartment, deleteDepartment, getDepartments, updateDepartment } from '@/api/department'
import { useToast } from '@/composables/useToast'
import { STATUS_DEPARTMENT_OPTIONS } from '../../constants/option'
import DepartmentForm from './DepartmentForm.vue'
import { getDepartmentById } from '../../api/department'

// =========================
// Stores, state and constants
// =========================
const toast = useToast()

const searchKeyword = ref('')
const isTableLoading = ref(false)
const departments = ref([])
const isShowDepartmentForm = ref(false)
const selectedDepartment = ref(null)

// Table column definitions
const departmentColumns = [
  { title: 'Mã phòng ban', dataIndex: 'code', key: 'code' },
  { title: 'Tên phòng ban', dataIndex: 'name', key: 'name' },
  { title: 'Trưởng phòng', dataIndex: 'departmentHeadId', key: 'departmentHeadId' },
  { title: 'Phòng ban cha', dataIndex: 'parentDepartmentId', key: 'parentDepartmentId' },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', width: 170 },
  { title: '', dataIndex: 'actions', key: 'actions', width: 90 }
]

// =========================
// UI actions
// =========================
const handleAddDepartment = () => {
  selectedDepartment.value = null
  isShowDepartmentForm.value = true
}

const handleCloseDepartmentForm = () => {
  isShowDepartmentForm.value = false
  selectedDepartment.value = null
}

const handleEditDepartment = async (department) => {
  if (!department?.id) {
    return
  }

  try {
    const response = await getDepartmentById(department.id)
    selectedDepartment.value = response.data
    isShowDepartmentForm.value = true
  } catch (error) {
    console.error('Failed to fetch department details:', error)
    toast.error('Không thể tải chi tiết phòng ban')
  }
}

const handleSubmitDepartmentForm = async (formData) => {
  if (formData?.id) {
    await updateExistingDepartment(formData)
  } else {
    await createNewDepartment(formData)
  }
}

const handleDeleteDepartment = async (department) => {
  if (!department?.id) {
    return
  }

  if (!window.confirm(`Bạn có chắc chắn muốn xóa phòng ban ${department.name}?`)) {
    return
  }

  try {
    await deleteDepartment(department.id)
    toast.success('Xóa phòng ban thành công')
    await fetchDepartments()
  } catch (error) {
    console.error('Failed to delete department:', error)
    toast.error('Không thể xóa phòng ban')
  }
}


// =========================
// API actions
// =========================

/**
 * Lấy danh sách phòng ban từ API và cập nhật state. Hiển thị toast lỗi nếu có lỗi xảy ra.
 * @returns {Promise<void>} trả về promise để có thể theo dõi trạng thái hoàn thành của việc fetch dữ liệu
 * @throws {Error} ném lỗi nếu có lỗi xảy ra trong quá trình gọi API
 */
const fetchDepartments = async () => {
  isTableLoading.value = true

  try {
    const response = await getDepartments()
    departments.value = Array.isArray(response?.data) ? response.data : []
  } catch (error) {
    console.error('Failed to fetch departments:', error)
    departments.value = []
    toast.error('Không thể tải danh sách phòng ban')
  } finally {
    isTableLoading.value = false
  }
}

/**
 * Tạo phòng ban mới bằng API. Hiển thị toast thành công hoặc lỗi tương ứng.
 * @param {Object} departmentData - Dữ liệu phòng ban cần tạo
 * @returns {Promise<void>} trả về promise để có thể theo dõi trạng thái hoàn thành của việc tạo phòng ban
 * @throws {Error} ném lỗi nếu có lỗi xảy ra trong quá trình gọi API
 */

const createNewDepartment = async (departmentData) => {
  try {
    await createDepartment(departmentData)
    toast.success('Phòng ban mới đã được tạo thành công')
    await fetchDepartments() // Tải lại danh sách phòng ban sau khi tạo mới
    handleCloseDepartmentForm()
  } catch (error) {
    console.error('Failed to create department:', error)
    toast.error('Không thể tạo phòng ban mới')
  }
}

const updateExistingDepartment = async (departmentData) => {
  try {
    await updateDepartment(departmentData.id, departmentData)
    toast.success('Cập nhật phòng ban thành công')
    await fetchDepartments()
    handleCloseDepartmentForm()
  } catch (error) {
    console.error('Failed to update department:', error)
    toast.error('Không thể cập nhật phòng ban')
  }
}



// =========================
// Lifecycle
// =========================
onMounted(() => {
  void fetchDepartments()
})
</script>

<template>
  <section class="department-page">
    <!-- Title -->
    <div class="department-page__header">
      <div>
        <h1 class="department-page__title">Phòng ban</h1>
        <p class="department-page__subtitle">Danh sách phòng ban toàn công ty</p>
      </div>

      <BaseButton iconClass="fas fa-plus" @click="handleAddDepartment">
        Thêm phòng ban
      </BaseButton>
    </div>

    <div class="department-page__wrap">
      <div class="toolbar">
        <div class="toolbar-search">
          <BaseInput
            v-model="searchKeyword"
            placeholder="Tìm theo mã hoặc tên phòng ban"
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

      <!-- Department Table -->
      <BaseTable
        class="department-table"
        :columns="departmentColumns"
        :data="departments"
        :loading="isTableLoading"
        :pagination="false"
        row-key="id"
      >
        <template #bodyCell="{ column, text, record }">
          <template v-if="column.dataIndex === 'status'">
            <span
              class="status-badge"
              :class="Number(text) === 1 ? 'status-badge--active' : 'status-badge--inactive'"
            >
              {{ STATUS_DEPARTMENT_OPTIONS.find((opt) => opt.value === Number(text))?.label || '--' }}
            </span>
          </template>
          <template v-else-if="column.key === 'actions'">
            <div class="row-action-buttons">
              <button
                v-permission="['Admin']"
                class="row-action-button"
                type="button"
                aria-label="Chỉnh sửa phòng ban"
                @click="handleEditDepartment(record)"
              >
                <i class="fa-solid fa-pen-to-square"></i>
              </button>
              <button
                v-permission="['Admin']"
                class="row-action-button row-action-button--delete"
                type="button"
                aria-label="Xóa phòng ban"
                @click="handleDeleteDepartment(record)"
              >
                <i class="fa-solid fa-trash"></i>
              </button>
            </div>
          </template>
          <template v-else>
            {{ text ?? '--' }}
          </template>
        </template>
      </BaseTable>
    </div>
  </section>

  <DepartmentForm
  v-if="isShowDepartmentForm"
  :initialData="selectedDepartment"
  @close="handleCloseDepartmentForm"
  @submit="handleSubmitDepartmentForm"
  />
</template>

<style scoped>
.department-page {
  display: grid;
  gap: 16px;
}

.department-page__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.department-page__title {
  margin: 0;
  font-size: 24px;
  font-weight: 800;
  color: var(--color-text-primary);
}

.department-page__subtitle {
  margin: 4px 0 0;
  color: #64748b;
  font-size: 14px;
}

.department-page__wrap {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.toolbar-icon:hover {
  border-color: #4387ee;
  color: #4387ee;
  background: #f1f7ff;
}

.department-page :deep(.department-table .ant-table-wrapper) {
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 14px 30px rgba(15, 23, 42, 0.1);
}

.department-page :deep(.department-table .ant-table-container) {
  border: 1px solid #e8edf5;
}

.row-action-buttons {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.department-page :deep(.department-table .row-action-button) {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: 1px solid transparent;
  border-radius: 6px;
  background: transparent;
  color: #4387ee;
  cursor: pointer;
  opacity: 0;
  transform: translateY(2px);
  transition: all 0.18s ease;
}

.department-page :deep(.department-table .row-action-button--delete) {
  color: #ef4444;
}

.department-page :deep(.department-table .row-action-button:hover) {
  background: #eff6ff;
  border-color: #bfdbfe;
  color: #4387ee;
}

.department-page :deep(.department-table .row-action-button.row-action-button--delete:hover) {
  background: #fee2e2;
  border-color: #fca5a5;
  color: #ef4444;
}

.department-page :deep(.department-table .ant-table-tbody > tr:hover .row-action-button) {
  opacity: 1;
  transform: translateY(0);
}

.department-page :deep(.department-table .ant-table-cell:last-child) {
  text-align: center;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 120px;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
}

.status-badge--active {
  color: #065f46;
  background: #d1fae5;
}

.status-badge--inactive {
  color: #92400e;
  background: #ffedd5;
}

@media (max-width: 992px) {
}

@media (max-width: 768px) {
  .department-page__header {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>