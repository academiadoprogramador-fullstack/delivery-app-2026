using DeliveryApp.Dominio.Modulos.Estabelecimentos;
using DeliveryApp.Infraestrutura.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infraestrutura.Modulos.Estabelecimentos;

public sealed class RepositorioEstabelecimentoEmOrm(
    DeliveryAppDbContext dbContext
) : RepositorioBaseEmOrm<Estabelecimento>(dbContext), IRepositorioEstabelecimento
{
    public Task<bool> ExistePorNomeComercialAsync(
        string nomeComercialNormalizado,
        Guid? idIgnorado = null,
        CancellationToken cancellationToken = default
    )
    {
        return registros.AnyAsync(
            e => e.NomeComercialNormalizado == nomeComercialNormalizado
                && (!idIgnorado.HasValue || e.Id != idIgnorado.Value),
            cancellationToken
        );
    }

    public Task<List<Estabelecimento>> SelecionarDisponiveisAsync(
        TimeOnly horarioAtual,
        CancellationToken cancellationToken = default
    )
    {
        return registros
            .AsNoTracking()
            .Where(e => e.Ativo && (
                e.HorarioAbertura < e.HorarioFechamento
                    ? horarioAtual >= e.HorarioAbertura && horarioAtual < e.HorarioFechamento
                    : horarioAtual >= e.HorarioAbertura || horarioAtual < e.HorarioFechamento
            ))
            .OrderBy(e => e.NomeComercial)
            .ToListAsync(cancellationToken);
    }

    public override Task<List<Estabelecimento>> SelecionarTodosAsync(
        CancellationToken cancellationToken = default
    )
    {
        return registros
            .AsNoTracking()
            .OrderBy(e => e.NomeComercial)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AtivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var estabelecimento = await registros.SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (estabelecimento is null)
            return false;

        estabelecimento.Ativar();
        await SalvarAlteracoesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DesativarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var estabelecimento = await registros.SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (estabelecimento is null)
            return false;

        estabelecimento.Desativar();
        await SalvarAlteracoesAsync(cancellationToken);
        return true;
    }
}
