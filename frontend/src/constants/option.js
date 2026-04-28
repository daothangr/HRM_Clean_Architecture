import { DEPARTMENT_STATUS, EMPLOYEE_STATUS, GENDER, ATTENDANCE_STATUS, LEAVE_REQUEST_STATUS, LEAVE_TYPE } from './enum'

export const GENDER_OPTIONS = Object.freeze([
  { label: 'Nam', value: GENDER.Male },
  { label: 'Nữ', value: GENDER.Female },
  { label: 'Khác', value: GENDER.Other },
])

export const STATUS_EMPLOYEE_OPTIONS = Object.freeze([
  { label: 'Đang làm việc', value: EMPLOYEE_STATUS.Active },
  { label: 'Thử việc', value: EMPLOYEE_STATUS.Probation },
  { label: 'Đã nghỉ việc', value: EMPLOYEE_STATUS.Resigned },
])

export const STATUS_DEPARTMENT_OPTIONS = Object.freeze([
  { label: 'Hoạt động', value: DEPARTMENT_STATUS.Active },
  { label: 'Đã ngừng hoạt động', value: DEPARTMENT_STATUS.Inactive },
])

export const ROLE_OPTIONS = Object.freeze([
  { label: 'Nhân viên', value: 'Employee' },
  { label: 'Quản lý', value: 'Manager' },
  { label: 'Admin', value: 'Admin' },
  { label: 'Giám đốc', value: 'Director' },
])

export const ATTENDANCE_STATUS_OPTIONS = Object.freeze([
  { label: 'Có mặt', value: ATTENDANCE_STATUS.Present },
  { label: 'Đi trễ', value: ATTENDANCE_STATUS.Late },
  { label: 'Về sớm', value: ATTENDANCE_STATUS.EarlyLeave },
  { label: 'Vắng mặt', value: ATTENDANCE_STATUS.Absent },
  { label: 'Nghỉ phép', value: ATTENDANCE_STATUS.Leave },
])

export const LEAVE_REQUEST_STATUS_OPTIONS = Object.freeze([
  { label: 'Đang chờ', value: LEAVE_REQUEST_STATUS.Pending },
  { label: 'Đã duyệt', value: LEAVE_REQUEST_STATUS.Approved },
  { label: 'Bị từ chối', value: LEAVE_REQUEST_STATUS.Rejected },
  { label: 'Đã hủy', value: LEAVE_REQUEST_STATUS.Cancelled },
])

export const LEAVE_TYPE_OPTIONS = Object.freeze([
  { label: 'Nghỉ hàng năm', value: LEAVE_TYPE.Annual },
  { label: 'Nghỉ ốm', value: LEAVE_TYPE.Sick },
  { label: 'Nghỉ không lương', value: LEAVE_TYPE.Unpaid },
  { label: 'Nghỉ khác', value: LEAVE_TYPE.Other },
])
