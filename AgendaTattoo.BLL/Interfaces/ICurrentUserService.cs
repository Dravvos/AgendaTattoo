namespace AgendaTattoo.BLL.Interfaces;

/// <summary>
/// Expõe a identidade do usuário autenticado da request atual (extraída do JWT).
/// Toda consulta a dados de um estúdio (clientes, agenda, serviços) deve ser filtrada
/// por StudioId a partir daqui — é a principal barreira contra vazamento de dados entre estúdios.
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    Guid? StudioId { get; }
    IReadOnlyList<string> Roles { get; }
}
