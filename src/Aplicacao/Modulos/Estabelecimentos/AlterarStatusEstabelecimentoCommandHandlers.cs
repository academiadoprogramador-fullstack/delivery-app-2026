using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Estabelecimentos;
using FluentResults;
using MediatR;

namespace DeliveryApp.Aplicacao.Modulos.Estabelecimentos;

public sealed record AtivarEstabelecimentoCommand(Guid EstabelecimentoId) : IRequest<Result>;
public sealed record DesativarEstabelecimentoCommand(Guid EstabelecimentoId) : IRequest<Result>;

public sealed class AtivarEstabelecimentoCommandHandler(
    IRepositorioEstabelecimento repositorioEstabelecimento,
    IProvedorDeUsuario provedorDeUsuario
) : IRequestHandler<AtivarEstabelecimentoCommand, Result>
{
    public async Task<Result> Handle(
        AtivarEstabelecimentoCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (command.EstabelecimentoId != provedorDeUsuario.Id)
            return Result.Fail(ErrosDeEstabelecimento.NaoAutorizado());

        bool alterado = await repositorioEstabelecimento.AtivarAsync(
            command.EstabelecimentoId,
            cancellationToken
        );

        return alterado
            ? Result.Ok()
            : Result.Fail(ErrosDeEstabelecimento.NaoEncontrado());
    }
}

public sealed class DesativarEstabelecimentoCommandHandler(
    IRepositorioEstabelecimento repositorioEstabelecimento,
    IProvedorDeUsuario provedorDeUsuario
) : IRequestHandler<DesativarEstabelecimentoCommand, Result>
{
    public async Task<Result> Handle(
        DesativarEstabelecimentoCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (command.EstabelecimentoId != provedorDeUsuario.Id)
            return Result.Fail(ErrosDeEstabelecimento.NaoAutorizado());

        bool alterado = await repositorioEstabelecimento.DesativarAsync(
            command.EstabelecimentoId,
            cancellationToken
        );

        return alterado
            ? Result.Ok()
            : Result.Fail(ErrosDeEstabelecimento.NaoEncontrado());
    }
}
