<script setup>
import dayjs from 'dayjs'
import { computed, onMounted, ref, watch } from 'vue'
import BaseTable from '@/components/base/BaseTable.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseDatePicker from '@/components/base/BaseDatePicker.vue'
import { useAuthStore } from '@/stores/auth'
import { getAttendanceRecords } from '@/api/attendance'
import { useToast } from '@/composables/useToast'
import { formatDate } from '@/utils/formatters'
import { ATTENDANCE_STATUS_OPTIONS } from '@/constants/option'

const authStore = useAuthStore()
const toast = useToast()

const searchKeyword = ref('')
const employeeCode = ref(authStore.employeeCode)
const dateFrom = ref(dayjs().startOf('month').toDate())
const dateTo = ref(dayjs().toDate())
const currentPage = ref(1)
const pageSize = ref(10)
const totalRecords = ref(0)
const isTableLoading = ref(false)
const attendanceData = ref([])

const attendanceColumns = [
  { title: 'Nhân viên', dataIndex: 'employeeName', key: 'employeeName' },
  { title: 'Phòng ban', dataIndex: 'departmentName', key: 'departmentName' },
  { title: 'Ngày', dataIndex: 'date', key: 'date' },
  { title: 'Giờ vào', dataIndex: 'checkInTime', key: 'checkInTime' },
  { title: 'Giờ ra', dataIndex: 'checkOutTime', key: 'checkOutTime' },
  { title: 'Giờ làm', dataIndex: 'workHours', key: 'workHours' },
  { title: 'Tăng ca', dataIndex: 'overtimeHours', key: 'overtimeHours' },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status' },
]

const tablePagination = computed(() => ({
  current: currentPage.value,
  pageSize: pageSize.value,
  total: totalRecords.value,
  showSizeChanger: true,
  position: ['bottomRight'],
  pageSizeOptions: ['10', '20', '50'],
  showTotal: (total, range) => `${range[0]}-${range[1]} / ${total}`,
}))


const formatApiDate = (value) => {
  if (!value) {
    return null
  }

  return dayjs(value).format('YYYY-MM-DD')
}

const formatTimeValue = (value) => {
  if (!value) {
    return '--'
  }

  return String(value).slice(0, 8)
}

/**
 * Hàm để tải dữ liệu chấm công từ API dựa trên các bộ lọc hiện tại
 * @returns {Promise<void>}
 */
const fetchAttendanceRecords = async () => {
  if (!dateFrom.value || !dateTo.value) {
    attendanceData.value = []
    totalRecords.value = 0
    return
  }

  isTableLoading.value = true

  try {
    const response = await getAttendanceRecords({
      from: formatApiDate(dateFrom.value),
      to: formatApiDate(dateTo.value),
      employeeCode: employeeCode.value,
      pageNumber: currentPage.value,
      pageSize: pageSize.value,
    })

    const paginationData = response?.data ?? {}
    attendanceData.value = paginationData.items ?? []
    totalRecords.value = paginationData.totalCount ?? 0
    currentPage.value = paginationData.pageNumber ?? currentPage.value
    pageSize.value = paginationData.pageSize ?? pageSize.value
  } catch (error) {
    console.error('Failed to fetch attendance records:', error)
    attendanceData.value = []
    totalRecords.value = 0
    toast.error('Không thể tải lịch sử chấm công')
  } finally {
    isTableLoading.value = false
  }
}

/**
 * Xử lý thay đổi trang của bảng
 * @param {Object} pagination - Thông tin trang mới
 */
const handleTableChange = async (pagination) => {
  const nextPageSize = pagination?.pageSize ?? pageSize.value
  const nextPage = pagination?.current ?? currentPage.value
  const isPageSizeChanged = nextPageSize !== pageSize.value

  pageSize.value = nextPageSize
  currentPage.value = isPageSizeChanged ? 1 : nextPage

  await fetchAttendanceRecords()
}

/**
 * Watchers để tự động tải lại dữ liệu khi thay đổi bộ lọc ngày tháng
 * 
 */
watch([dateFrom, dateTo], async () => {
  currentPage.value = 1
  await fetchAttendanceRecords()
})

onMounted(async () => {
  await fetchAttendanceRecords()
})


</script>

<template>
  <section class="attendance-history-page">
    <div class="main-content__title">
      <div class="title-name">Lịch sử chấm công</div>
    </div>

    <div class="main-content__wrap">
      <div class="toolbar">
        <div class="toolbar-search">
          <BaseInput 
            v-model="employeeCode"
            placeholder="Tìm theo mã nhân viên"
            iconClass="fa-solid fa-magnifying-glass"
          />
          <base-button @click="fetchAttendanceRecords">
            Tìm kiếm
          </base-button>
        </div>

        <div class="attendance-history__date-filters">
          <BaseDatePicker
            v-model:modelValue="dateFrom"
            class="attendance-history__date-picker"
            placeholder="Từ ngày"
          />
          <BaseDatePicker
            v-model:modelValue="dateTo"
            class="attendance-history__date-picker"
            placeholder="Đến ngày"
          />
        </div>
      </div>

      <BaseTable
        class="attendance-history-table"
        :columns="attendanceColumns"
        :data="attendanceData"
        :loading="isTableLoading"
        :pagination="tablePagination"
        row-key="id"
        :scroll = "{ y: 'calc(100vh - 375px)'}"
        @change="handleTableChange"
      >
        <template #bodyCell="{ column, text }">
          <template v-if="column.dataIndex === 'date'">
            {{ text ? formatDate(text) : '--' }}
          </template>
          <template v-else-if="column.dataIndex === 'checkInTime' || column.dataIndex === 'checkOutTime'">
            {{ formatTimeValue(text) }}
          </template>
          <template v-else-if="column.dataIndex === 'workHours' || column.dataIndex === 'overtimeHours'">
            {{ text ?? 0 }}
          </template>
          <template v-else-if="column.dataIndex === 'status'">
            {{ ATTENDANCE_STATUS_OPTIONS.find((opt) => opt.value === Number(text))?.label || '--' }}
          </template>
          <template v-else>
            {{ text || '--' }}
          </template>
        </template>
      </BaseTable>
    </div>
  </section>
</template>

<style scoped>
.attendance-history-page {
  display: grid;
  gap: 16px;
}

.attendance-history__date-filters {
  display: grid;
  grid-template-columns: repeat(2, minmax(160px, 220px));
  gap: 10px;
}

.attendance-history__date-picker {
  width: 100%;
}

.attendance-history-page :deep(.attendance-history-table .ant-table-wrapper) {
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 14px 30px rgba(15, 23, 42, 0.08);
}

.toolbar-search {
  display: flex;
  gap: 8px;
  align-items: flex-start;
}

.toolbar-search .base-button {
    margin-top: 9px;
    height: 32px;
}



</style>
