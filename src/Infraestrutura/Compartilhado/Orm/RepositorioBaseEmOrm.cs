using DeliveryApp.Dominio.Compartilhado;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DeliveryApp.Infraestrutura.Compartilhado.Orm;

public abstract class RepositorioBaseEmOrm<T>(DeliveryAppDbContext dbContext) where T : EntidadeBase<T>
{
    protected readonly DbSet<T> registros = dbContext.Set<T>();

    public async Task CadastrarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        registros.Add(entidade);

        await SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task<bool> EditarAsync(
        Guid id,
        T entidadeAtualizada,
        CancellationToken cancellationToken = default
    )
    {
        T? registroSelecionado = await SelecionarPorIdAsync(id, cancellationToken);

        if (registroSelecionado == null)
            return false;

        registroSelecionado.Atualizar(entidadeAtualizada);

        await SalvarAlteracoesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        T? TSelecionado = await SelecionarPorIdAsync(id, cancellationToken);

        if (TSelecionado == null)
            return false;

        registros.Remove(TSelecionado);

        await SalvarAlteracoesAsync(cancellationToken);

        return true;
    }

    public virtual async Task<T?> SelecionarPorIdAsync(Guid idSelecionado, CancellationToken cancellationToken = default)
    {
        return await registros.SingleOrDefaultAsync(c => c.Id == idSelecionado, cancellationToken);
    }

    public virtual async Task<List<T>> SelecionarTodosAsync(CancellationToken cancellationToken = default)
    {
        return await registros.ToListAsync(cancellationToken);
    }

    protected async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException excecao) when (
            excecao.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            dbContext.ChangeTracker.Clear();
            throw new ConflitoDePersistenciaException();
        }
    }
}
