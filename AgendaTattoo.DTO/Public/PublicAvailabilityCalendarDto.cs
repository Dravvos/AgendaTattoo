using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.DTO.Public
{
    public class PublicAvailabilityCalendarDto
    {
        public List<DayOfWeek>? WorkingDays { get; set; }
        public List<DateOnly>? BlockedDates { get; set; }
    }
}
