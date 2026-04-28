using System;
using System.Collections.Generic;
using System.Text;

namespace HRM.Domain.Enums
{
    public enum AttendanceStatus : byte
    {
        Present = 1,   // Có mặt
        Late = 2,      // Đi trễ
        EarlyLeave = 3, // Về sớm
        Absent = 4,     // Vắng
        Leave = 5       // Nghỉ phép
    }
}
