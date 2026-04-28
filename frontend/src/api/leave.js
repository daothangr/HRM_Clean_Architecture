import axios from './axios'

/**
 * Get all leave requests with pagination and optional status filter
 * @param {Object} params - Query parameters
 * @param {number} params.pageNumber - Page number (1-based)
 * @param {number} params.pageSize - Page size
 * @param {number} [params.status] - Leave request status filter (optional)
 * @returns {Promise} Response data containing paginated leave requests
 */
export const getLeaveRequests = (params) => {
  return axios.get('/leaves', { params })
}

/**
 * Get leave request by ID
 * @param {number} id - Leave request ID
 * @returns {Promise} Response data containing leave request details
 */
export const getLeaveRequestById = (id) => {
  return axios.get(`/leaves/${id}`)
}

/**
 * Create a new leave request
 * @param {Object} data - Leave request data
 * @returns {Promise} Response data with created leave request ID
 */
export const createLeaveRequest = (data) => {
  return axios.post('/leaves', data)
}

/**
 * Update an existing leave request
 * @param {number} id - Leave request ID
 * @param {Object} data - Updated leave request data
 * @returns {Promise} Response from update operation
 */
export const updateLeaveRequest = (id, data) => {
  return axios.put(`/leaves/${id}`, data)
}

/**
 * Delete a leave request
 * @param {number} id - Leave request ID
 * @returns {Promise} Response from delete operation
 */
export const cancelLeaveRequest = (id) => {
  return axios.put(`/leaves/${id}/cancel`)
}

/**
 * Process (approve/reject) a leave request
 * @param {number} id - Leave request ID
 * @param {Object} data - Process data {approve: boolean, comment: string}
 * @returns {Promise} Response from process operation
 */
export const processLeaveRequest = (id, data) => {
  return axios.post(`/leaves/${id}/process`, data)
}
