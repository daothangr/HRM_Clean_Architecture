import { createApp } from 'vue'
import '@/assets/css/styles.css'
import '@/assets/css/variable.css'
import '@/assets/css/toolbar.css'
import '@/assets/css/page.css'
import '@/assets/css/form.css'
import App from './App.vue'
import router from '@/routers'
import { createPinia } from 'pinia'
import permissionDirective from '@/directives/permission'

import Antd from 'ant-design-vue'
import 'ant-design-vue/dist/reset.css'

const app = createApp(App)
const pinia = createPinia()

app.use(Antd)
app.use(pinia)
app.use(router)
app.directive('permission', permissionDirective)
app.mount('#app')
