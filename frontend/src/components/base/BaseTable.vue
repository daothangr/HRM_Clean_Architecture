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

const handleTableChange = (pagination, filters, sorter, extra) => {
  emit('change', pagination, filters, sorter, extra)
}
</script>

<template>
  <div class="table-container">
    <a-table
        :columns="columns"
        :row-key="rowKey"
        :data-source="data"
        :pagination="pagination"
        :loading="loading"
        :scroll="scroll"
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
  </div>
  
</template>

<style scoped>
.table-container {
    height: 100%;
    display: flex;
    flex-direction: column;
}


:deep(.ant-table-wrapper) {
  flex: 1;
  display: flex;
  flex-direction: column;
}

:deep(.ant-spin-nested-loading),
:deep(.ant-spin-container),
:deep(.ant-table) {
  flex: 1;
  display: flex;
  flex-direction: column;
}

:deep(.ant-table-container) {
  flex: 1;
  overflow: auto;
}

:deep(.ant-table-pagination) {
  position: sticky;
  bottom: 0;
  background: white;
  z-index: 10;
  margin: 0;
  padding: 12px 16px;
}
</style>



