using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.Data.Entities
{
    public enum AppointmentStatus
    {
        PendingConfirmation = 0,
        Confirmed = 1,
        Cancelled = 2,
        Completed = 3,
        NoShow = 4,
    }
}
