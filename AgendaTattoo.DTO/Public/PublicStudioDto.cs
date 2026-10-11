namespace AgendaTattoo.DTO.Public;

public class PublicStudioDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Slug { get; set; }

    /// <summary>IANA timezone id do estúdio (ex.: "America/Sao_Paulo") — a página pública exibe os horários nesse fuso.</summary>
    public string? TimeZoneId { get; set; }

}
