<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import BaseInput from '@/components/base/BaseInput.vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const router = useRouter()
const isUserMenuOpen = ref(false)

const displayRole = computed(() => authStore.roles?.[0] || 'Employee')

const avatarText = computed(() => {
  const name = authStore.fullName || authStore.email || 'U'

  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map(part => part[0]?.toUpperCase())
    .join('') || 'U'
})

const toggleUserMenu = () => {
  isUserMenuOpen.value = !isUserMenuOpen.value
}

const closeUserMenu = () => {
  isUserMenuOpen.value = false
}

const handleLogout = async () => {
  authStore.logout()
  closeUserMenu()
  await router.push('/login')
}

const handleClickOutside = (event) => {
  if (!event.target.closest('.profile-menu')) {
    closeUserMenu()
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<template>
  <header class="header">
    <div class="header-branch">
      <div class="logo-wrap" aria-hidden="true">
        <i class="fa-solid fa-building"></i>
      </div>
      <div class="brand-title">HRM Portal</div>
    </div>

    <div class="header-search">
      <BaseInput
        placeholder="Search people, leave, policies"
        iconClass="fa-solid fa-magnifying-glass"
      />
    </div>

    <div class="header-action">
      <button class="notification-btn" type="button" aria-label="Notifications">
        <i class="fa-regular fa-bell"></i>
        <span class="badge">2</span>
      </button>

      <div class="profile-menu" :class="{ open: isUserMenuOpen }">
        <button
          type="button"
          class="profile"
          aria-label="User menu"
          @click.stop="toggleUserMenu"
        >
          <div class="avatar avatar--text" aria-hidden="true">{{ avatarText }}</div>
          <div class="profile-info">
            <p class="name">{{ authStore.fullName || 'Người dùng' }}</p>
            <p class="email">{{ authStore.email || 'No email' }}</p>
          </div>
          <svg
            class="chevron"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke-width="1.5"
            stroke="currentColor"
            aria-hidden="true"
          >
            <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7" />
          </svg>
        </button>

        <transition name="dropdown-fade">
          <div v-if="isUserMenuOpen" class="profile-dropdown" @click.stop>
            <div class="profile-dropdown__header">
              <div>
                <p class="profile-dropdown__name">{{ authStore.fullName || 'Người dùng' }}</p>
                <p class="profile-dropdown__email">{{ authStore.email || 'No email' }}</p>
                <span class="profile-dropdown__role">{{ displayRole }}</span>
              </div>
            </div>

            <button type="button" class="logout-btn" @click="handleLogout">
              <i class="fa-solid fa-right-from-bracket"></i>
              <span>Đăng xuất</span>
            </button>
          </div>
        </transition>
      </div>
    </div>
  </header>
</template>

<style scoped>
.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  min-height: 4rem;
  padding: 1rem 1.125rem;
  background: #ffffff;
  border-bottom: 1px solid #e9edf3;
  border-radius: 16px;
  box-shadow: 0 12px 32px rgba(15, 23, 42, 0.06);
  position: relative;
  z-index: 5;
}

.header-branch {
  display: flex;
  align-items: center;
  padding: 0px 8px;
  gap: 0.625rem;
  min-width: 10.5rem;
}

.logo-wrap {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: 0.45rem;
  background: var(--color-branch-primary);
  color: #ffffff;
}

.logo-wrap svg {
  width: 1.1rem;
  height: 1.1rem;
}

.brand-title {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #1f2937;
}

.header-search {
  flex: 1;
  max-width: 28rem;
  height: 2.35rem;
  border-radius: 0.4rem;
  display: flex;
  align-items: center;
  padding: 0 0.625rem;
  color: #8893a5;
  background: #ffffff;
}

.search-icon {
  width: 1rem;
  height: 1rem;
  flex: 0 0 auto;
}

.header-search input {
  border: none;
  outline: none;
  width: 100%;
  margin-left: 0.5rem;
  color: #2f3a4b;
  font-size: 0.9rem;
  background: transparent;
}

.header-search input::placeholder {
  color: #9aa5b5;
}

.header-action {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.profile-menu {
  position: relative;
}

.notification-btn {
  position: relative;
  width: 2rem;
  height: 2rem;
  border: none;
  background: transparent;
  color: #2f3a4b;
  display: grid;
  place-items: center;
  padding: 0;
  cursor: pointer;
}

.fa-bell {
  font-size: 16px;
}

.notification-btn svg {
  width: 1.5rem;
  height: 1.5rem;
}

.badge {
  position: absolute;
  top: 0.15rem;
  right: 0.08rem;
  min-width: 0.85rem;
  height: 0.85rem;
  border-radius: 9999px;
  display: grid;
  place-items: center;
  font-size: 0.58rem;
  line-height: 1;
  font-weight: 600;
  color: #ffffff;
  background: #ef4444;
}

.profile {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  border: none;
  background: transparent;
  padding: 0.25rem 0.35rem;
  border-radius: 9999px;
  cursor: pointer;
}

.avatar {
  width: 1.9rem;
  height: 1.9rem;
  border-radius: 9999px;
  object-fit: cover;
}

.avatar--text {
  display: grid;
  place-items: center;
  background: linear-gradient(135deg, #1d4ed8, #3b82f6);
  color: #ffffff;
  font-size: 0.78rem;
  font-weight: 700;
  letter-spacing: 0.04em;
}

.avatar--large {
  width: 3rem;
  height: 3rem;
  flex: 0 0 auto;
}

.profile-info {
  line-height: 1.3;
  text-align: left;
  min-width: 0;
}

.name,
.role,
.email {
  margin: 0;
}

.name {
  max-width: 13rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-weight: 500;
  color: var(--color-text-primary);
}

.email {
  max-width: 13rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 12px;
  color: var(--color-text-placeholder);
}

.profile-dropdown {
  position: absolute;
  right: 0;
  top: calc(100% + 0.5rem);
  width: 18rem;
  padding: 0.9rem;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.96);
  border: 1px solid #e8edf4;
  box-shadow: 0 20px 40px rgba(15, 23, 42, 0.12);
  backdrop-filter: blur(10px);
  z-index: 20;
}

.profile-dropdown__header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding-bottom: 0.85rem;
  border-bottom: 1px solid #edf1f7;
}

.profile-dropdown__name {
  margin: 0;
  font-size: 0.98rem;
  font-weight: 700;
  color: #162033;
}

.profile-dropdown__email {
  margin: 0.2rem 0 0;
  color: var(--color-text-placeholder);
}

.profile-dropdown__role {
  display: inline-flex;
  margin-top: 0.45rem;
  padding: 0.2rem 0.55rem;
  border-radius: 9999px;
  background: #eef4ff;
  color: #1d4ed8;
  font-size: 11px;
  font-weight: 700;
}

.logout-btn {
  width: 100%;
  margin-top: 0.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.6rem;
  border: none;
  border-radius: 12px;
  padding: 0.8rem 1rem;
  background: #fff1f2;
  color: #be123c;
  font-weight: 700;
  cursor: pointer;
  transition: background-color 0.2s ease, transform 0.2s ease;
}

.logout-btn:hover {
  background: #ffe4e6;
}

.dropdown-fade-enter-active,
.dropdown-fade-leave-active {
  transition: opacity 0.18s ease, transform 0.18s ease;
}

.dropdown-fade-enter-from,
.dropdown-fade-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}

.chevron {
  width: 1rem;
  height: 1rem;
  color: var(--color-text-placeholder);
}

@media (max-width: 900px) {
  .header {
    flex-wrap: wrap;
  }

  .header-search {
    order: 3;
    width: 100%;
    max-width: none;
  }
}

@media (max-width: 580px) {
  .email,
  .chevron {
    display: none;
  }

  .header {
    padding: 0.5rem 0.625rem;
  }

  .profile-dropdown {
    width: min(18rem, calc(100vw - 1.5rem));
  }
}

</style>