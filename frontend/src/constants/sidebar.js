export const SIDEBAR_MENU = [
  {
    title: "Thông tin cá nhân",
    icon: "fa-user",
    path: "/profile",
    roles: ["Employee", "Admin", "Manager", "Director"]
  },
  {
    title: "Báo cáo",
    icon: "fa-house",
    path: "/dashboard",
    roles: ["Admin", "Manager", "Director"]
  },
  {
    title: "Nhân viên",
    icon: "fa-users",
    path: "/employees",
    roles: ["Admin", "Manager"]
  },
  {
    title: "Phòng ban",
    icon: "fa-building",
    path: "/departments",
    roles: ["Admin", "Manager", "Director"]
  },
  {
    title: "Chấm công",
    icon: "fa-clock",
    path: "/attendance",
    roles: ["Admin", "Manager", "Director", "Employee"],
    children: [
      {
        title: "Chấm công",
        path: "/attendance/check-in",
        roles: ["Manager", "Employee", "Admin"]
      },
      {
        title: "Lịch sử chấm công",
        path: "/attendance/history",
        roles: ["Admin", "Manager", "Director", "Employee"]
      }
    ]
  },
  {
    title: "Nghỉ phép",
    icon: "fa-calendar-days",
    path: "/leaves",
    roles: ["Admin", "Manager", "Director", "Employee"]
  }
];