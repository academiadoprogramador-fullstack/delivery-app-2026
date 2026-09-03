using DeliveryApp.Aplicacao.Modulos.Estabelecimentos.DTOs;
using DeliveryApp.Dominio.Modulos.Estabelecimentos;
using FluentResults;
using MediatR;

namespace DeliveryApp.Aplicacao.Modulos.Estabelecimentos;

public sealed record ListarEstabelecimentosQuery : IRequest<Result<List<EstabelecimentoDto>>>;

public sealed class ListarEstabelecimentosQueryHandler(
    IRepositorioEstabelecimento repositorioEstabelecimento
) : IRequestHandler<ListarEstabelecimentosQuery, Result<List<EstabelecimentoDto>>>
{
    public async Task<Result<List<EstabelecimentoDto>>> Handle(
        ListarEstabelecimentosQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var estabelecimentos = await repositorioEstabelecimento.SelecionarTodosAsync(cancellationToken);
        return Result.Ok(estabelecimentos.Select(Mapear).ToList());
    }

    internal static EstabelecimentoDto Mapear(Estabelecimento estabelecimento) => new(
        estabelecimento.Id,
        estabelecimento.NomeComercial,
        estabelecimento.Documento,
        estabelecimento.Endereco,
        estabelecimento.Telefone,
        estabelecimento.HorarioAbertura,
        estabelecimento.HorarioFechamento,
        estabelecimento.AreaAtendimento,
        estabelecimento.Ativo
    );
}
