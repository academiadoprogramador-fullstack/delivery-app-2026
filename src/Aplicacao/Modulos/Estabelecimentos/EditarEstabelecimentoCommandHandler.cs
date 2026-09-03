using DeliveryApp.Aplicacao.Modulos.Estabelecimentos.DTOs;
using DeliveryApp.Dominio.Compartilhado;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Estabelecimentos;
using FluentResults;
using MediatR;

namespace DeliveryApp.Aplicacao.Modulos.Estabelecimentos;

public sealed record EditarEstabelecimentoCommand(
    Guid EstabelecimentoId,
    string NomeComercial,
    string Documento,
    string Endereco,
    string Telefone,
    TimeOnly HorarioAbertura,
    TimeOnly HorarioFechamento,
    string AreaAtendimento
) : IRequest<Result<EstabelecimentoDto>>;

public sealed class EditarEstabelecimentoCommandHandler(
    IRepositorioEstabelecimento repositorioEstabelecimento,
    IProvedorDeUsuario provedorDeUsuario
) : IRequestHandler<EditarEstabelecimentoCommand, Result<EstabelecimentoDto>>
{
    public async Task<Result<EstabelecimentoDto>> Handle(
        EditarEstabelecimentoCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (command.EstabelecimentoId != provedorDeUsuario.Id)
            return Result.Fail(ErrosDeEstabelecimento.NaoAutorizado());

        var existente = await repositorioEstabelecimento.SelecionarPorIdAsync(
            command.EstabelecimentoId,
            cancellationToken
        );

        if (existente is null)
            return Result.Fail(ErrosDeEstabelecimento.NaoEncontrado());

        var atualizado = new Estabelecimento(
            command.EstabelecimentoId,
            command.NomeComercial,
            command.Documento,
            command.Endereco,
            command.Telefone,
            command.HorarioAbertura,
            command.HorarioFechamento,
            command.AreaAtendimento,
            existente.Ativo
        );

        var erros = atualizado.Validar();

        if (erros.Count > 0)
            return Result.Fail(ErrosDeEstabelecimento.Validacao(erros));

        bool nomeJaCadastrado = await repositorioEstabelecimento.ExistePorNomeComercialAsync(
            atualizado.NomeComercialNormalizado,
            command.EstabelecimentoId,
            cancellationToken
        );

        if (nomeJaCadastrado)
            return Result.Fail(ErrosDeEstabelecimento.NomeComercialDuplicado());

        try
        {
            bool editado = await repositorioEstabelecimento.EditarAsync(
                command.EstabelecimentoId,
                atualizado,
                cancellationToken
            );

            if (!editado)
                return Result.Fail(ErrosDeEstabelecimento.NaoEncontrado());

            return Result.Ok(ListarEstabelecimentosQueryHandler.Mapear(atualizado));
        }
        catch (ConflitoDePersistenciaException)
        {
            return Result.Fail(ErrosDeEstabelecimento.NomeComercialDuplicado());
        }
    }
}
