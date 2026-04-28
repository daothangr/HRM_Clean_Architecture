import { createRouter, createWebHistory } from 'vue-router'
import { setupRouterGuards } from '@/routers/guards'

// Import Views
import DashboardView from '@/views/dashboard/index.vue'
import ProfileView from '@/views/profile/index.vue'
import EmployeeView from '@/views/employee/index.vue'
import DepartmentsView from '@/views/department/index.vue'
import AttendanceView from '@/views/attendance/CheckInView.vue'
import AttendanceHistoryView from '@/views/attendance/AttendanceHistoryView.vue'
import LeaveView from '@/views/leave/index.vue'
import LoginView from '@/views/auth/LoginView.vue'
import DefaultLayout from '@/layouts/DefaultLayout.vue'

const routes = [
  {
    path: '/',
    component: DefaultLayout,
    meta: {
      requiresAuth: true
    },
    children: [
      {
        path: '',
        redirect: '/profile'
      },
      {
        path: 'profile',
        name: 'Profile',
        component: ProfileView,
        meta: {
          title: 'Profile',
          roles: ['Employee', 'Admin', 'Manager', 'Director']
        }

      },
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: DashboardView,
        meta: {
          title: 'Dashboard',
          roles: ['Admin', 'Manager', 'Director']
        }
      },
      {
        path: 'employees',
        name: 'Employee',
        component: EmployeeView,
        meta: {
          title: 'Employee Management',
          roles: ['Admin', 'Manager']
        }
      },
      {
        path: 'departments',
        name: 'Departments',
        component: DepartmentsView,
        meta: {
          title: 'Department Management',
          roles: ['Admin', 'Manager', 'Director']
        }
      },
      {
        path: 'attendance',
        redirect: '/attendance/check-in'
      },
      {
        path: 'attendance/check-in',
        name: 'AttendanceCheckIn',
        component: AttendanceView,
        meta: {
          title: 'Attendance',
          roles: ['Manager', 'Employee', 'Admin']
        }
      },
      {
        path: 'attendance/history',
        name: 'AttendanceHistory',
        component: AttendanceHistoryView,
        meta: {
          title: 'Attendance History',
          roles: ['Admin', 'Manager', 'Director', 'Employee']
        }
      },
      {
        path: 'leaves',
        name: 'Leaves',
        component: LeaveView,
        meta: {
          title: 'Leave Management',
          roles: ['Admin', 'Manager', 'Director', 'Employee']
        }
      }
    ]
  },
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
    meta: {
      title: 'Đăng nhập',
      guestOnly: true
    }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

setupRouterGuards(router)

export default router
