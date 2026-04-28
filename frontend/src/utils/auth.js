
/** Hàm kiểm tra xem vai trò người dùng có nằm trong danh sách các vai trò được phép truy cập hay không.
 * @param {string} userRole - Vai trò của người dùng hiện tại.
 * @param {string[]} allowedRoles - Mảng các vai trò được phép truy cập.
 * @returns {boolean} Trả về true nếu vai trò người dùng có trong danh sách allowedRoles, ngược lại trả về false.
 */
export const hasRole = (userRole, allowedRoles = []) => {
  if (!userRole) return false;
  return allowedRoles.includes(userRole);
};

/**
 * Hàm kiểm tra xem người dùng có ít nhất một trong các vai trò được phép truy cập hay không.
 * @param {string[]} userRoles - Mảng các vai trò của người dùng hiện tại.
 * @param {string[]} allowedRoles - Mảng các vai trò được phép truy cập.
 * @returns {boolean} Trả về true nếu người dùng có ít nhất một vai trò trong danh sách allowedRoles, ngược lại trả về false.
 */
export const hasAnyRole = (userRoles = [], allowedRoles = []) => {
  return userRoles.some(role => allowedRoles.includes(role));
};

/**
 * Hàm kiểm tra xem người dùng có tất cả các vai trò được yêu cầu hay không.
 * @param {string[]} userRoles - Mảng các vai trò của người dùng hiện tại.
 * @param {string[]} requiredRoles - Mảng các vai trò được yêu cầu.
 * @returns {boolean} Trả về true nếu người dùng có tất cả các vai trò trong danh sách requiredRoles, ngược lại trả về false.
 */
export const hasAllRoles = (userRoles = [], requiredRoles = []) => {
  return requiredRoles.every(role => userRoles.includes(role));
};