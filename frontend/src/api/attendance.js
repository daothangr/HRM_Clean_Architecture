import axios from './axios'

export const upsertAttendance = (attendanceData) => {
  return axios.post('/AttendanceRecords', attendanceData)
}

export const getAttendanceRecords = (params = {}) => {
  return axios.get('/AttendanceRecords', { params })
}