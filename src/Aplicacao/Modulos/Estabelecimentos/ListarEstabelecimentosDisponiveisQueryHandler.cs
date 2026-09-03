using DeliveryApp.Aplicacao.Modulos.Estabelecimentos.DTOs;
using DeliveryApp.Dominio.Compartilhado;
using DeliveryApp.Dominio.Modulos.Estabelecimentos;
using FluentResults;
using MediatR;

namespace DeliveryApp.Aplicacao.Modulos.Estabelecimentos;

public sealed record ListarEstabelecimentosDisponiveisQuery
    : IRequest<Result<List<EstabelecimentoDto>>>;

public sealed class ListarEstabelecimentosDisponiveisQueryHandler(
    IRepositorioEstabelecimento repositorioEstabelecimento,
    IProvedorDeHorario provedorDeHorario
) : IRequestHandler<ListarEstabelecimentosDisponiveisQuery, Result<List<EstabelecimentoDto>>>
{
    public async Task<Result<List<EstabelecimentoDto>>> Handle(
        ListarEstabelecimentosDisponiveisQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var estabelecimentos = await repositorioEstabelecimento.SelecionarDisponiveisAsync(
            provedorDeHorario.ObterHorarioAtual(),
            cancellationToken
        );

        return Result.Ok(estabelecimentos.Select(ListarEstabelecimentosQueryHandler.Mapear).ToList());
    }
}
