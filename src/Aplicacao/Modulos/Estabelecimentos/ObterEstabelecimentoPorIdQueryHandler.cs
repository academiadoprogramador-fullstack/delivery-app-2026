using DeliveryApp.Aplicacao.Modulos.Estabelecimentos.DTOs;
using DeliveryApp.Dominio.Modulos.Estabelecimentos;
using FluentResults;
using MediatR;

namespace DeliveryApp.Aplicacao.Modulos.Estabelecimentos;

public sealed record ObterEstabelecimentoPorIdQuery(Guid EstabelecimentoId)
    : IRequest<Result<EstabelecimentoDto>>;

public sealed class ObterEstabelecimentoPorIdQueryHandler(
    IRepositorioEstabelecimento repositorioEstabelecimento
) : IRequestHandler<ObterEstabelecimentoPorIdQuery, Result<EstabelecimentoDto>>
{
    public async Task<Result<EstabelecimentoDto>> Handle(
        ObterEstabelecimentoPorIdQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var estabelecimento = await repositorioEstabelecimento.SelecionarPorIdAsync(
            query.EstabelecimentoId,
            cancellationToken
        );

        if (estabelecimento is null)
            return Result.Fail(ErrosDeEstabelecimento.NaoEncontrado());

        return Result.Ok(ListarEstabelecimentosQueryHandler.Mapear(estabelecimento));
    }
}
