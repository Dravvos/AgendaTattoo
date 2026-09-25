namespace AgendaTattoo.Data.Entities;

/// <summary>
/// Janela de trabalho recorrente de um artista em um dia da semana (ex.: Terça 10:00–19:00).
/// Usada para calcular os horários disponíveis mostrados na agenda pública de agendamento.
/// Simplificação de MVP: um único bloco contínuo por dia (sem intervalo de almoço, sem exceções
/// pontuais como feriados/folgas — isso fica para uma próxima iteração).
/// </summary>
public class ArtistWorkingHours
{
    public Guid Id { get; set; }

    public Guid ArtistId { get; set; }
    public ApplicationUser Artist { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
