using DeliveryApp.Aplicacao.Modulos.Estabelecimentos.DTOs;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Estabelecimentos;
using FluentResults;
using MediatR;

namespace DeliveryApp.Aplicacao.Modulos.Estabelecimentos;

public sealed record AutenticarEstabelecimentoCommand(
    string Email,
    string Senha
) : IRequest<Result<AccessTokenDoEstabelecimentoDto>>;

public sealed record AccessTokenDoEstabelecimentoDto(
    Guid EstabelecimentoId,
    string Token,
    DateTime DataExpiracaoEmUtc
);

public sealed class AutenticarEstabelecimentoCommandHandler(
    IGerenciadorDeIdentidade gerenciadorDeIdentidade,
    IEmissorDeTokens emissorDeTokens,
    IRepositorioEstabelecimento repositorioEstabelecimento
) : IRequestHandler<AutenticarEstabelecimentoCommand, Result<AccessTokenDoEstabelecimentoDto>>
{
    public async Task<Result<AccessTokenDoEstabelecimentoDto>> Handle(
        AutenticarEstabelecimentoCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var usuario = await gerenciadorDeIdentidade.ChecarValidadeDeSenhaAsync(
            command.Email,
            command.Senha,
            TipoUsuario.Estabelecimento
        );

        if (usuario is null)
            return Result.Fail(ErrosDeEstabelecimento.CredenciaisInvalidas());

        var estabelecimento = await repositorioEstabelecimento.SelecionarPorIdAsync(
            usuario.Id,
            cancellationToken
        );

        if (estabelecimento is null)
            return Result.Fail(ErrosDeEstabelecimento.CredenciaisInvalidas());

        var accessToken = emissorDeTokens.CriarToken(
            usuario.Id,
            usuario.Email,
            TipoUsuario.Estabelecimento
        );

        return Result.Ok(new AccessTokenDoEstabelecimentoDto(
            usuario.Id,
            accessToken.Token,
            accessToken.DataExpiracaoEmUtc
        ));
    }
}
