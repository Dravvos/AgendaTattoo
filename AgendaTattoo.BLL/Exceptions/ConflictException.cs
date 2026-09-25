namespace AgendaTattoo.BLL.Exceptions;

/// <summary>A operação conflita com o estado atual dos dados (ex.: horário já ocupado, concorrência).</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
