<script setup>
import { computed, onMounted, ref, useAttrs, watch } from 'vue';
import { getDropdownOptions } from '@/api/dropdown';

defineOptions({ inheritAttrs: false });
const attrs = useAttrs();

/**
 * Props cấu hình BaseDropBox.
 *
 * @property {boolean} required - Hiển thị dấu bắt buộc.
 * @property {string | number | undefined} modelValue - Giá trị đang chọn.
 * @property {string} placeholder - Placeholder hiển thị trong ô chọn.
 * @property {string} route - Route API dùng để lấy danh sách option.
 * @property {Array} options - Danh sách option truyền thủ công.
 */
const props = defineProps({
  required: {
    type: Boolean,
    default: false,
  },
  modelValue: {
    type: [String, Number],
    default: undefined,
  },
  placeholder: {
    type: String,
    default: '',
  },
  route: {
    type: String,
    default: '',
  },
  options: {
    type: Array,
    default: () => [],
  },
});

const emit = defineEmits(['update:modelValue', 'change']);

/**
 * Computed property quản lý giá trị đang chọn của BaseDropBox.
 *
 * @returns {string | number | undefined}
 */
const value = computed({
  // Ant Select only shows placeholder when value is undefined.
  get: () => (props.modelValue === '' || props.modelValue === null ? undefined : props.modelValue),
  set: (newValue) => {
    emit('update:modelValue', newValue ?? '');
  },
});

const fetchedOptions = ref([]);

/**
 * Chuẩn hóa dữ liệu option từ API hoặc dữ liệu truyền vào.
 *
 * @param {unknown} item - Một phần tử dữ liệu thô.
 * @returns {{ label: string, value: string | number }}
 */
const normalizeOption = (item) => {
  if (item !== null && typeof item === 'object') {
    const record = item;
    const label = record.label ?? record.name ?? record.fullName;
    const value =  record.id ?? record.value ?? record.code;

    return {
      label: label ?? String(value ?? ''),
      value: value ?? label ?? '',
    };
  }

  return {
    label: String(item),
    value: item,
  };
};

/**
 * Tải danh sách option từ API theo route được truyền vào.
 *
 * Nếu không có route thì component sẽ dùng options truyền tay.
 */
const loadOptions = async () => {
  if (!props.route) {
    fetchedOptions.value = [];
    return;
  }

  const data = await getDropdownOptions(props.route);
  fetchedOptions.value = data.map(normalizeOption);
};

const displayedOptions = computed(() => {
  return props.route ? fetchedOptions.value : props.options;
});

/**
 * Phát sự kiện khi người dùng chọn giá trị mới.
 *
 * @param {string | number} newValue - Giá trị được chọn.
 */
const handleChange = (newValue) => {
  emit('change', newValue);
};


/**
 * Lọc option theo text người dùng nhập.
 *
 * @param {string} input - Chuỗi tìm kiếm.
 * @param {Object} option - Option đang được kiểm tra.
 * @returns {boolean}
 */
const filterOption = (input, option) => {
  const target = String(option?.label ?? option?.value ?? '');
  return target.toLowerCase().includes(input.toLowerCase());
};

/**
 * Tải lại danh sách option khi route thay đổi hoặc khi component khởi tạo.
 */
watch(
  () => props.route,
  () => {
    void loadOptions();
  },
  { immediate: true },
);

onMounted(() => {
  void loadOptions();
});
</script>

<template>
  <div class="base-dropbox">
    <!-- Label -->
    <div class="base-dropbox__label">
      <span class="base-dropbox__label-left">
        <slot></slot>
        <span v-if="required" class="base-dropbox__required">*</span>
      </span>
      <span class="base-dropbox__label-right">
        <slot name="label-right"></slot>
      </span>
    </div>

    <!-- Content -->
    <div class="base-dropbox__content">
      <a-select
        v-model:value="value"
        class="base-dropbox__control"
        show-search
        :placeholder="placeholder"
        :options="displayedOptions"
        :filter-option="filterOption"
        v-bind="attrs"
        @change="handleChange"
      ></a-select>

    </div>
  </div>
</template>

<style scoped>
/* Wrapper */
.base-dropbox {
  display: flex;
  flex: 1;
  min-width: 0;
  flex-direction: column;
  gap: 6px;
}

/* Label */
.base-dropbox__label {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-primary);
}

/* Required star */
.base-dropbox__required {
  color: #ef4444;
  margin-left: 4px;
}

.base-dropbox__label-right {
  font-size: 14px;
  color: #b2b2b2;
  white-space: nowrap;
}

/* Content */
.base-dropbox__content {
  height: 36px;
  width: 100%;
  box-sizing: border-box;
  border: 1px solid #e5e7eb;
  border-radius: 4px;
}

.base-dropbox__content:hover,
.base-dropbox__content:focus-within {
  border-color: var(--color-branch-primary) !important;
  box-shadow: none !important;
}

.base-dropbox__control {
  height: 100%;
  width: 100%;
}

:deep(.base-dropbox__control .ant-select-selector) {
  border: none !important;
  box-shadow: none !important;
  background: transparent !important;
  padding: 0 12px !important;
  height: 34px !important;
  align-items: center;
}

:deep(.base-dropbox__control .ant-select-selection-placeholder) {
  color: var(--color-text-placeholder) !important;
  font-weight: 300;
  font-family: 'Inter', system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  font-size: 14px;
  opacity: 1;
}

</style>

