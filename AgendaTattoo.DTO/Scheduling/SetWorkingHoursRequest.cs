using System.ComponentModel.DataAnnotations;

namespace AgendaTattoo.DTO.Scheduling;

public class SetWorkingHoursRequest
{
    /// <summary>Lista completa dos dias/faixas de trabalho — substitui integralmente a configuração atual.</summary>
    [Required]
    public List<WorkingHoursEntryRequest> Days { get; set; } = [];
}

public class WorkingHoursEntryRequest
{
    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }
}
