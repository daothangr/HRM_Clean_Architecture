<script setup>
// =========================
// Imports
// =========================
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseTable from '@/components/base/BaseTable.vue'
import { getLeaveRequests, createLeaveRequest, updateLeaveRequest, cancelLeaveRequest, processLeaveRequest, getLeaveRequestById } from '@/api/leave'
import { formatDate } from '@/utils/formatters'
import LeaveForm from './LeaveForm.vue'
import { computed, onMounted, ref, watch } from 'vue'
import { LEAVE_TYPE_OPTIONS, LEAVE_REQUEST_STATUS_OPTIONS } from '../../constants/option'
import { LEAVE_REQUEST_STATUS, LEAVE_TYPE } from '../../constants/enum'
import { useToast } from '@/composables/useToast'
import { useAuthStore } from '@/stores/auth'
import dayjs from 'dayjs'
import { scrollToTop } from '@/utils/scroll'

// =========================
// Stores, state and constants
// =========================
const authStore = useAuthStore()
const searchKeyword = ref('')
const leaveData = ref([])
const isTableLoading = ref(false)
const isShowLeaveForm = ref(false)
const selectedLeave = ref(null)
const currentPage = ref(1)
const pageSize = ref(10)
const totalLeaves = ref(0)
const selectedStatus = ref(null)
const toast = useToast()

const LEAVE_STATUS_CLASS_MAP = Object.freeze({
  [LEAVE_REQUEST_STATUS.Pending]: 'leave-status-badge--pending',
  [LEAVE_REQUEST_STATUS.Approved]: 'leave-status-badge--approved',
  [LEAVE_REQUEST_STATUS.Rejected]: 'leave-status-badge--rejected',
  [LEAVE_REQUEST_STATUS.Cancelled]: 'leave-status-badge--cancelled',
})

// Table column definitions
const leaveColumns = [
  { title: 'Nhân viên', dataIndex: 'employeeName', key: 'employeeName' },
  { title: 'Phòng ban', dataIndex: 'departmentName', key: 'departmentName' },
  { title: 'Loại nghỉ', dataIndex: 'leaveType', key: 'leaveType' },
  { title: 'Ngày bắt đầu', dataIndex: 'startDate', key: 'startDate' },
  { title: 'Ngày kết thúc', dataIndex: 'endDate', key: 'endDate' },
  { title: 'Số ngày', dataIndex: 'totalDays', key: 'totalDays' },
  { title: 'Cấp duyệt', dataIndex: 'currentApprovalLevel', key: 'currentApprovalLevel' },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status' },
  { title: '', key: 'actions', dataIndex: 'actions', width: 60, fixed: 'right' },
]

// =========================
// UI actions
// =========================
const handleOpenFormAddLeave = () => {
  scrollToTop()
  selectedLeave.value = null
  isShowLeaveForm.value = true
}

const handleCloseLeaveForm = () => {
  isShowLeaveForm.value = false
  selectedLeave.value = null
}

const handleStatusFilterChange = () => {
  currentPage.value = 1
  fetchLeaveRequests()
}

// =========================
// Table configs
// =========================
const tablePagination = computed(() => ({
  current: currentPage.value,
  pageSize: pageSize.value,
  total: totalLeaves.value,
  showSizeChanger: true,
  position: ['bottomRight'],
  pageSizeOptions: ['10', '20', '50'],
  showTotal: (total, range) => `${range[0]}-${range[1]} / ${total}`
}))

// =========================
// API actions
// =========================
/**
 * Fetch leave requests with pagination and optional status filter
 */
const fetchLeaveRequests = async () => {
  isTableLoading.value = true

  try {
    const params = {
      pageNumber: currentPage.value,
      pageSize: pageSize.value,
    }

    // Add status filter if selected
    if (selectedStatus.value !== null) {
      params.status = selectedStatus.value
    }

    const response = await getLeaveRequests(params)
    const payload = response?.data ?? response ?? {}
    const items = Array.isArray(payload)
      ? payload
      : payload.items ?? []
    const totalCount = payload.totalCount ??  items.length

    leaveData.value = items
    totalLeaves.value = totalCount
    currentPage.value = payload.pageNumber ?? currentPage.value
    pageSize.value = payload.pageSize ?? pageSize.value
    console.log('Fetched leave requests:', items)
  } catch (error) {
    console.error('Failed to fetch leave requests:', error)
    leaveData.value = []
    totalLeaves.value = 0
  } finally {
    isTableLoading.value = false
  }
}

/**
 * Handle table pagination/size changes
 */
const handleTableChange = async (pagination) => {
  const nextPageSize = pagination?.pageSize ?? pageSize.value
  const nextPage = pagination?.current ?? currentPage.value
  const isPageSizeChanged = nextPageSize !== pageSize.value

  pageSize.value = nextPageSize
  currentPage.value = isPageSizeChanged ? 1 : nextPage

  await fetchLeaveRequests()
}

/**
 * Handle form submission - create or update leave request
 */
const handleSubmitLeaveForm = async (formData) => {
  try {
    if (formData.id) {
      // Update existing leave request
      await updateLeaveRequest(formData.id, formData)
      console.log('Leave request updated successfully')
      toast.success('Cập nhật đơn nghỉ thành công')
    } else {
      // Create new leave request
      await createLeaveRequest(formData)
      console.log('Leave request created successfully')
      toast.success('Tạo đơn nghỉ thành công')
    }
    // Close form and refresh list
    handleCloseLeaveForm()
    currentPage.value = 1
    await fetchLeaveRequests()
  } catch (error) {
    console.error('Failed to save leave request:', error)
    toast.error('Có lỗi xảy ra khi lưu đơn nghỉ')
  }
}

/**
 * Handle approve leave request
 */
const handleApproveLeave = async (leave) => {
  if (!leave?.id) {
    return
  }

  const comment = prompt('Nhập bình luận (tùy chọn):', '')
  if (comment === null) {
    return
  }

  try {
    await processLeaveRequest(leave.id, {
      approve: true,
      comment: comment || ''
    })
    console.log('Leave request approved successfully')
    toast.success('Duyệt đơn nghỉ thành công')
    await fetchLeaveRequests()
  } catch (error) {
    console.error('Failed to approve leave request:', error)
    toast.error('Có lỗi xảy ra khi duyệt đơn nghỉ')
  }
}

/**
 * Handle reject leave request
 */
const handleRejectLeave = async (leave) => {
  if (!leave?.id) {
    return
  }

  const comment = prompt('Nhập lý do từ chối:', '')
  if (comment === null) {
    return
  }

  if (!comment.trim()) {
    toast.warning('Vui lòng nhập lý do từ chối')
    return
  }

  try {
    await processLeaveRequest(leave.id, {
      approve: false,
      comment: comment
    })
    console.log('Leave request rejected successfully')
    toast.success('Từ chối đơn nghỉ thành công')
    await fetchLeaveRequests()
  } catch (error) {
    console.error('Failed to reject leave request:', error)
    toast.error('Có lỗi xảy ra khi từ chối đơn nghỉ')
  }
}

const handleCancelLeave = async (leave) => {
  if (!leave?.id) {
    return
  }

  const confirmCancel = confirm('Bạn có chắc chắn muốn hủy đơn nghỉ này?')
  if (!confirmCancel) {
    return
  }

  try {
    await cancelLeaveRequest(leave.id)
    console.log('Leave request cancelled successfully')
    toast.success('Hủy đơn nghỉ thành công')
    await fetchLeaveRequests()
  } catch (error) {
    console.error('Failed to cancel leave request:', error)
    toast.error('Có lỗi xảy ra khi hủy đơn nghỉ')
  }
}

/**
 * Check if user can approve leave requests
 */
const canApprove = computed(() => {
  const roles = authStore.user?.roles || []
  console.log('User roles:', roles)
  return roles.includes('Admin') || roles.includes('Manager') || roles.includes('Director')
})

/**
 * Check if leave request is still pending
 */
const isPending = (leave) => {
  return leave.status === LEAVE_REQUEST_STATUS.Pending
}

// =========================
// Lifecycle
// =========================
onMounted(() => {
  fetchLeaveRequests()
})

/**
 * Watch for status filter changes
 */
watch(() => selectedStatus.value, () => {
  handleStatusFilterChange()
})
</script>

<template>
  <section class="leave-page">
    <!-- Title -->
    <div class="main-content__title">
      <div class="title-name">Đơn nghỉ phép</div>
      <div class="title-button">
        <base-button iconClass="fas fa-plus" @click="handleOpenFormAddLeave">
          Tạo đơn nghỉ
        </base-button>
      </div>
    </div>

    <div class="main-content__wrap">
      <!-- Status Filter Cards -->
      <div class="status-filter-container">
        <button
          class="status-filter-card"
          :class="{ 'status-filter-card--active': selectedStatus === null }"
          @click="selectedStatus = null"
        >
          <div class="status-filter-label">Tất cả</div>
        </button>

        <button
          class="status-filter-card status-filter-card--approved"
          :class="{ 'status-filter-card--active': selectedStatus === LEAVE_REQUEST_STATUS.Approved }"
          @click="selectedStatus = LEAVE_REQUEST_STATUS.Approved"
        >
          <div class="status-filter-label">Hoàn thành</div>
        </button>

        <button
          class="status-filter-card status-filter-card--rejected"
          :class="{ 'status-filter-card--active': selectedStatus === LEAVE_REQUEST_STATUS.Rejected }"
          @click="selectedStatus = LEAVE_REQUEST_STATUS.Rejected"
        >
          <div class="status-filter-label">Từ chối</div>
        </button>

        <button
          class="status-filter-card status-filter-card--pending"
          :class="{ 'status-filter-card--active': selectedStatus === LEAVE_REQUEST_STATUS.Pending }"
          @click="selectedStatus = LEAVE_REQUEST_STATUS.Pending"
        >
          <div class="status-filter-label">Đang xử lý</div>
        </button>

        <button
          class="status-filter-card status-filter-card--cancelled"
          :class="{ 'status-filter-card--active': selectedStatus === LEAVE_REQUEST_STATUS.Cancelled }"
          @click="selectedStatus = LEAVE_REQUEST_STATUS.Cancelled"
        >
          <div class="status-filter-label">Đã hủy</div>
        </button>
      </div>

      <!-- Leave Table -->
      <base-table
        class="leave-table"
        :columns="leaveColumns"
        :data="leaveData"
        :loading="isTableLoading"
        :pagination="tablePagination"
        :scroll="{ y: 'calc(100vh - 390px)'}"
        row-key="id"
        @change="handleTableChange"
      >
        <template #bodyCell="{ column, text, record }">
          <template v-if="column.dataIndex === 'status'">
            <span class="leave-status-badge" :class="LEAVE_STATUS_CLASS_MAP[Number(text)] || 'leave-status-badge--default'">
              {{ LEAVE_REQUEST_STATUS_OPTIONS.find(option => option.value === text)?.label || '--' }}
            </span>
          </template>
          <template v-else-if="column.dataIndex === 'leaveType'">
            {{ LEAVE_TYPE_OPTIONS.find(option => option.value === text)?.label || '--' }}
          </template>
          <template v-else-if="column.dataIndex === 'startDate'">
            {{ text ? formatDate(text) : '--' }}
          </template>
          <template v-else-if="column.dataIndex === 'endDate'">
            {{ text ? formatDate(text) : '--' }}
          </template>
          <template v-else-if="column.dataIndex === 'totalDays'">
            {{ text || '--' }}
          </template>
          <template v-else-if="column.key === 'actions'">
            <div class="row-action-buttons">
              <button
                v-if="canApprove && isPending(record)"
                class="row-action-button row-action-button--approve"
                type="button"
                title="Duyệt đơn"
                @click="handleApproveLeave(record)"
              >
                <i class="fa-solid fa-check"></i>
              </button>
              <button
                v-if="canApprove && isPending(record)"
                class="row-action-button row-action-button--reject"
                type="button"
                title="Từ chối"
                @click="handleRejectLeave(record)"
              >
                <i class="fa-solid fa-times"></i>
              </button>
              <button
                v-permission="['Employee']"
                class="row-action-button row-action-button--reject"
                type="button"
                title="Hủy đơn"
                @click="handleCancelLeave(record)"
              >
                <i class="fa-solid fa-times"></i>
              </button>
            </div>
          </template>
          <template v-else>
            {{ text || '--' }}
          </template>
        </template>
      </base-table>
    </div>

    <LeaveForm
      v-if="isShowLeaveForm"
      :initialData="selectedLeave"
      @close="handleCloseLeaveForm"
      @submit="handleSubmitLeaveForm"
    />
  </section>
</template>

<style scoped>
.leave-page {
  display: grid;
  gap: 16px;
}

.status-filter-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 14px;
}

.status-filter-card {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: center;
  min-height: 72px;
  padding: 16px 18px;
  border: 1px solid rgba(148, 163, 184, 0.22);
  border-radius: 16px;
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease, border-color 0.2s ease, background 0.2s ease;
  box-shadow: 0 10px 20px rgba(15, 23, 42, 0.08);
  position: relative;
  overflow: hidden;
}

.status-filter-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 16px 28px rgba(15, 23, 42, 0.12);
}

.status-filter-card--active {
  transform: translateY(-3px);
  border-color: rgba(255, 255, 255, 0.6);
  box-shadow: 0 18px 30px rgba(15, 23, 42, 0.16);
}

.status-filter-card:not(.status-filter-card--active) {
  opacity: 0.7;
}

.status-filter-card:not(.status-filter-card--active):hover {
  opacity: 0.85;
}

.status-filter-card:nth-child(1) {
  background: linear-gradient(135deg, #3b82f6 0%, #60a5fa 100%);
  color: white;
}

.status-filter-card:nth-child(1).status-filter-card--active {
  background: linear-gradient(135deg, #2563eb 0%, #3b82f6 100%);
  color: white;
}

.status-filter-card--approved {
  background: linear-gradient(135deg, #10b981 0%, #34d399 100%);
  color: white;
}

.status-filter-card--approved.status-filter-card--active {
  background: linear-gradient(135deg, #059669 0%, #10b981 100%);
  color: white;
}

.status-filter-card--rejected {
  background: linear-gradient(135deg, #ef4444 0%, #f87171 100%);
  color: white;
}

.status-filter-card--rejected.status-filter-card--active {
  background: linear-gradient(135deg, #dc2626 0%, #ef4444 100%);
  color: white;
}

.status-filter-card--pending {
  background: linear-gradient(135deg, #f59e0b 0%, #fbbf24 100%);
  color: white;
}

.status-filter-card--pending.status-filter-card--active {
  background: linear-gradient(135deg, #d97706 0%, #f59e0b 100%);
  color: white;
}

.status-filter-card--cancelled {
  background: linear-gradient(135deg, #64748b 0%, #94a3b8 100%);
  color: white;
}

.status-filter-card--cancelled.status-filter-card--active {
  background: linear-gradient(135deg, #475569 0%, #64748b 100%);
  color: white;
}

.status-filter-label {
  font-size: 15px;
  font-weight: 700;
  letter-spacing: 0.2px;
  white-space: nowrap;
}

.toolbar {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.toolbar-filters {
  display: flex;
  gap: 12px;
  flex: 1;
  min-width: 300px;
}

.toolbar-filters > * {
  min-width: 200px;
}

.toolbar-actions {
  display: flex;
  gap: 8px;
}

.toolbar-icon {
  width: 40px;
  height: 40px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: white;
  color: #64748b;
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 16px;
}

.toolbar-icon:hover {
  border-color: #4387ee;
  color: #4387ee;
  background: #f1f7ff;
}

.leave-page :deep(.leave-table .row-action-button--approve) {
  color: #16a34a;
}

.leave-page :deep(.leave-table .row-action-button--reject) {
  color: #dc2626;
}

.leave-page :deep(.leave-table .ant-table-wrapper) {
  height: 560px;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 14px 30px rgba(15, 23, 42, 0.1);
}

.leave-page :deep(.leave-table .ant-spin-nested-loading),
.leave-page :deep(.leave-table .ant-spin-container) {
  height: 100%;
}

.leave-page :deep(.leave-table .ant-table) {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.leave-page :deep(.leave-table .ant-table-container) {
  flex: 1;
  min-height: 0;
  border: 1px solid #e8edf5;
}

.leave-page :deep(.leave-table .ant-pagination) {
  position: sticky;
  bottom: 0;
  z-index: 2;
  margin: 0;
  padding: 12px 16px;
  background: #ffffff;
  border-top: 1px solid #e8edf5;
}

.leave-page :deep(.leave-table .ant-table-cell:last-child) {
  text-align: center;
}

.leave-status-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 110px;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
  line-height: 1.4;
}

.leave-status-badge--pending {
  color: #d97706;
  background: #fef3c7;
}

.leave-status-badge--approved {
  color: #059669;
  background: #d1fae5;
}

.leave-status-badge--rejected {
  color: #dc2626;
  background: #fee2e2;
}

.leave-status-badge--cancelled {
  color: #6b7280;
  background: #f3f4f6;
}

.leave-status-badge--default {
  color: #334155;
  background: #e2e8f0;
}

@media (max-width: 1024px) {
  .toolbar-filters {
    width: 100%;
    flex: 1;
  }

  .toolbar-filters > * {
    flex: 1;
  }

  .status-filter-container {
    grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  }
}

@media (max-width: 768px) {
  .leave-page :deep(.leave-table .ant-table-wrapper) {
    height: 500px;
  }

  .toolbar {
    flex-direction: column;
    align-items: flex-start;
  }

  .toolbar-filters {
    width: 100%;
    flex-direction: column;
  }

  .toolbar-filters > * {
    width: 100%;
    min-width: unset;
  }

  .status-filter-container {
    grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  }

  .status-filter-card {
    min-height: 60px;
    padding: 12px 14px;
  }

  .status-filter-label {
    font-size: 13px;
  }

}
</style>
