export const isEmptyValue = (value) => {
  return (
    value === null ||
    value === undefined ||
    (typeof value === 'string' && String(value).trim() === '') ||
    (Array.isArray(value) && value.length === 0)
  )
}
