namespace AgendaTattoo.BLL.Exceptions;

/// <summary>Usuário autenticado, mas sem permissão para a operação (ex.: artista tentando ver a agenda de outro).</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
