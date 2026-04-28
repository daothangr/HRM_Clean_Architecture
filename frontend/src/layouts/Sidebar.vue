<script setup>
import { SIDEBAR_MENU } from "@/constants/sidebar"
import { useAuthStore } from "@/stores/auth"
import { useRoute } from "vue-router"
import { computed, ref } from "vue"
import { hasAnyRole } from "../utils/auth"

const authStore = useAuthStore()
const route = useRoute()

// Theo dõi trạng thái mở rộng của các nhóm menu có con
const expandedGroups = ref({})

/*
  Lọc menu dựa trên vai trò người dùng và cấu trúc phân cấp.
  Chỉ hiển thị các mục menu mà vai trò của người dùng có quyền truy cập.
*/
const menu = computed(() =>
  SIDEBAR_MENU.filter(item => hasAnyRole(authStore.roles, item.roles)).map(item => ({
    ...item,
    children: item.children?.filter(child => hasAnyRole(authStore.roles, child.roles)) || []
  }))
)

const isMenuGroupActive = (item) => item.children.some(child => route.path.startsWith(child.path))
const isGroupOpen = (item) => expandedGroups.value[item.path] ?? isMenuGroupActive(item)
const toggleGroup = (item) => {
  expandedGroups.value[item.path] = !isGroupOpen(item)
}
</script>
<template>
  <aside class="sidebar">
    <ul class="menu-list">
      <li v-for="item in menu" :key="item.path || item.title" class="menu-item">
        <template v-if="item.children.length > 0">
          <button
            type="button"
            :class="['menu-link', 'menu-link-group', { active: isMenuGroupActive(item) }]"
            @click="toggleGroup(item)"
          >
            <i :class="`fa ${item.icon}`"></i>
            <span>{{ item.title }}</span>
            <i :class="['fa', 'menu-chevron', isGroupOpen(item) ? 'fa-chevron-up' : 'fa-chevron-down']"></i>
          </button>

          <ul v-if="isGroupOpen(item)" class="sub-menu-list">
            <li v-for="child in item.children" :key="child.path" class="sub-menu-item">
              <router-link :to="child.path" class="sub-menu-link">
                <span>{{ child.title }}</span>
              </router-link>
            </li>
          </ul>
        </template>

        <router-link v-else :to="item.path" class="menu-link">
          <i :class="`fa ${item.icon}`"></i>
          <span>{{ item.title }}</span>
        </router-link>
      </li>
    </ul>
  </aside>
</template>

<style scoped>
.sidebar {
  width: 240px;
  height: 100%;
  min-height: 0;
  overflow: auto;
  background: #f9fafc;
  border-radius: 14px;
  padding: 14px 0px;
  border: 1px solid #eef1f5;
  box-shadow: 0 18px 40px rgba(15, 23, 42, 0.05);
}

.menu-list {
  list-style: none;
  margin: 0;
  padding: 12px 0px;
}

.menu-item {
  margin-bottom: 6px;
  padding: 0px 10px;
  box-sizing: border-box;
}

.menu-link {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  border: none;
  background: transparent;
  padding: 11px 12px;
  border-radius: 6px;
  color: #202531;
  text-decoration: none;
  font-size: 16px;
  font-weight: 400;
  line-height: 1.2;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.menu-link i {
  width: 14px;
  text-align: center;
  font-size: 14px;
}

.menu-link span {
  display: inline-block;
}

.menu-link:hover {
  background: #edf4fc;
}

.menu-link.router-link-active {
  background: var(--color-branch-primary);
  color: #ffffff;
  font-weight: 600;
}

.menu-link-group {
  cursor: pointer;
}

.menu-chevron {
  margin-left: auto;
  font-size: 12px;
}

.menu-link-group.active {
  background: #edf4fc;
}

.sub-menu-list {
  list-style: none;
  margin: 6px 0 0;
  padding: 0 0 0 30px;
}

.sub-menu-item {
  margin-bottom: 4px;
}

.sub-menu-link {
  display: block;
  padding: 8px 12px;
  border-radius: 6px;
  color: #3a4354;
  text-decoration: none;
  font-size: 14px;
  line-height: 1.2;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.sub-menu-link:hover {
  background: #edf4fc;
}

.sub-menu-link.router-link-active {
  background: var(--color-branch-primary);
  color: #ffffff;
  font-weight: 600;
}

@media (max-width: 900px) {
  .sidebar {
    width: 100%;
    min-height: auto;
    max-height: none;
  }
}
</style>