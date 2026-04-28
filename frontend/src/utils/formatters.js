/**
 * Hàm format ngày tháng
 *
 * @param {string | number | Date} value - Giá trị ngày cần format.
 * @param {string} [locale='vi-VN'] - Mã ngôn ngữ dùng để format ngày.
 * @returns {string} Chuỗi ngày đã format theo locale.
 */
export const formatDate = (value, locale = 'vi-VN') => {
  if (!value) {
    return '-'
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleDateString(locale)
}

/**
 * Hàm format số tiền
 *
 * @param {string | number | null | undefined} value - Giá trị số tiền cần format.
 * @param {string} [locale='vi-VN'] - Mã ngôn ngữ dùng để format số tiền.
 * @param {string} [currencyLabel='VND'] - Nhãn đơn vị tiền tệ đi kèm.
 * @returns {string} Chuỗi số tiền đã format theo locale và kèm nhãn đơn vị tiền tệ.
 */
export const formatMoney = (value, locale = 'vi-VN', currencyLabel = 'VND') => {
  if (value === null || value === undefined || value === '') {
    return '-'
  }

  const amount = Number(value)
  if (Number.isNaN(amount)) {
    return value
  }

  return `${amount.toLocaleString(locale)} ${currencyLabel}`
}

/**
 * Chuyển đổi giá trị thành mảng
 * @param {*} value 
 * @returns 
 */
export const normalizeToArray = (value) => {
  if (Array.isArray(value)) {
    return value
  }

  if (typeof value === 'string') {
    return [value]
  }

  if (value && typeof value === 'object') {
    if (Array.isArray(value.roles)) {
      return value.roles
    }

    if (typeof value.roles === 'string') {
      return [value.roles]
    }
  }

  return []
}