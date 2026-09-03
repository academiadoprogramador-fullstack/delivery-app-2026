using DeliveryApp.Dominio.Compartilhado;

namespace DeliveryApp.Dominio.Modulos.Estabelecimentos;

public interface IRepositorioEstabelecimento : IRepositorio<Estabelecimento>
{
    Task<bool> ExistePorNomeComercialAsync(
        string nomeComercialNormalizado,
        Guid? idIgnorado = null,
        CancellationToken cancellationToken = default
    );

    Task<List<Estabelecimento>> SelecionarDisponiveisAsync(
        TimeOnly horarioAtual,
        CancellationToken cancellationToken = default
    );

    Task<bool> AtivarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DesativarAsync(Guid id, CancellationToken cancellationToken = default);
}
