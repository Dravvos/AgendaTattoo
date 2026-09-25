namespace AgendaTattoo.BLL.Exceptions;

/// <summary>Recurso não encontrado — ou não existe, ou pertence a outro estúdio/artista (mesma resposta nos dois casos).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
