<script setup>
import { computed } from 'vue'

const emit = defineEmits(['change'])

const props = defineProps({
  columns: {
    type: Array,
    default: () => []
  },
  data: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  rowKey: {
    type: [String, Function],
    default: 'id'
  },
  pagination: {
    type: [Boolean, Object],
    default: false
  },
  scroll: {
    type: Object,
    default: null
  }
})

const resolvedScroll = computed(() => props.scroll ?? { x: '100vh-100px', y: 'calc(100vh - 360px)' })

const handleTableChange = (pagination, filters, sorter, extra) => {
  emit('change', pagination, filters, sorter, extra)
}
</script>

<template>
  <a-table
    :columns="columns"
    :row-key="rowKey"
    :data-source="data"
    :pagination="pagination"
    :loading="loading"
    :scroll="resolvedScroll"
    @change="handleTableChange"
  >
    <template #bodyCell="{ column, text, record }">
      <slot name="bodyCell" :column="column" :text="text" :record="record">
        <template v-if="typeof text === 'object' && text !== null">
          {{ JSON.stringify(text) }}
        </template>
        <template v-else>
          {{ text }}
        </template>
      </slot>
    </template>
  </a-table>
</template>



