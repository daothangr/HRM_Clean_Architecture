<script setup>
import Header from './Header.vue'
import Sidebar from './Sidebar.vue'
</script>

<template>
  <div class="app-shell">
    <div class="app-shell__header">
      <Header />
    </div>
    <div class="app-shell__body">
      <Sidebar />
      <main class="app-shell__content">
        <router-view />
      </main>
    </div>
  </div>
</template>

<style scoped>
.app-shell {
  height: 100vh;
  width: 100%;
  overflow: hidden;
  display: grid;
  grid-template-rows: auto minmax(0, 1fr);
  background:
    radial-gradient(circle at top left, rgba(67, 135, 238, 0.12), transparent 34%),
    linear-gradient(180deg, #f6f8fc 0%, #edf2f8 100%);
}

.app-shell__header {
  padding: 1rem 1rem 0;
}

.app-shell__body {
  display: flex;
  gap: 1rem;
  padding: 1rem;
  min-height: 0;
  overflow: hidden;
}

.app-shell__content {
  flex: 1;
  min-width: 0;
  min-height: 0;
  overflow: auto;
  border-radius: 14px;
  padding: 1.5rem;
  background: rgba(255, 255, 255, 0.72);
  backdrop-filter: blur(10px);
  box-shadow: 0 18px 50px rgba(15, 23, 42, 0.06);
}

@media (max-width: 900px) {
  .app-shell {
    height: auto;
    min-height: 100vh;
  }

  .app-shell__header {
    padding: 0.75rem 0.75rem 0;
  }

  .app-shell__body {
    flex-direction: column;
    overflow: visible;
  }

  .app-shell__content {
    overflow: visible;
  }
}
</style>