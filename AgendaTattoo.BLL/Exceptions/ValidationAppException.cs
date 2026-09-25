namespace AgendaTattoo.BLL.Exceptions;

/// <summary>Regra de negócio violada (além da validação de DataAnnotations, que já é tratada pelo [ApiController]).</summary>
public class ValidationAppException : Exception
{
    public ValidationAppException(string message) : base(message)
    {
    }
}
