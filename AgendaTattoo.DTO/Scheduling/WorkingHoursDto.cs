namespace AgendaTattoo.DTO.Scheduling;

public class WorkingHoursDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
