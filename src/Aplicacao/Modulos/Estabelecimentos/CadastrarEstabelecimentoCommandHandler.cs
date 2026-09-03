using DeliveryApp.Aplicacao.Modulos.Estabelecimentos.DTOs;
using DeliveryApp.Dominio.Compartilhado;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Estabelecimentos;
using FluentResults;
using MediatR;

namespace DeliveryApp.Aplicacao.Modulos.Estabelecimentos;

public sealed record CadastrarEstabelecimentoCommand(
    string NomeComercial,
    string Documento,
    string Endereco,
    string Telefone,
    TimeOnly HorarioAbertura,
    TimeOnly HorarioFechamento,
    string AreaAtendimento,
    string Email,
    string Senha
) : IRequest<Result<EstabelecimentoCadastradoDto>>;

public sealed class CadastrarEstabelecimentoCommandHandler(
    IRepositorioEstabelecimento repositorioEstabelecimento,
    IGerenciadorDeIdentidade gerenciadorDeIdentidade
) : IRequestHandler<CadastrarEstabelecimentoCommand, Result<EstabelecimentoCadastradoDto>>
{
    public async Task<Result<EstabelecimentoCadastradoDto>> Handle(
        CadastrarEstabelecimentoCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var estabelecimento = new Estabelecimento(
            Guid.CreateVersion7(),
            command.NomeComercial,
            command.Documento,
            command.Endereco,
            command.Telefone,
            command.HorarioAbertura,
            command.HorarioFechamento,
            command.AreaAtendimento
        );

        var erros = estabelecimento.Validar();

        if (erros.Count > 0)
            return Result.Fail(ErrosDeEstabelecimento.Validacao(erros));

        bool nomeJaCadastrado = await repositorioEstabelecimento.ExistePorNomeComercialAsync(
            estabelecimento.NomeComercialNormalizado,
            cancellationToken: cancellationToken
        );

        if (nomeJaCadastrado)
            return Result.Fail(ErrosDeEstabelecimento.NomeComercialDuplicado());

        try
        {
            await gerenciadorDeIdentidade.CadastrarAsync(
                estabelecimento.Id,
                command.Email,
                command.Senha,
                TipoUsuario.Estabelecimento
            );

            await repositorioEstabelecimento.CadastrarAsync(estabelecimento, cancellationToken);

            return Result.Ok(new EstabelecimentoCadastradoDto(
                estabelecimento.Id,
                estabelecimento.NomeComercial
            ));
        }
        catch (ValidacaoDeIdentidadeException excecao)
        {
            return Result.Fail(ErrosDeEstabelecimento.ValidacaoDeIdentidade(excecao.Campo, excecao.Message));
        }
        catch (ConflitoDeIdentidadeException excecao)
        {
            return Result.Fail(ErrosDeEstabelecimento.ConflitoDeIdentidade(excecao.Message));
        }
        catch (ConflitoDePersistenciaException)
        {
            await gerenciadorDeIdentidade.ExcluirAsync(estabelecimento.Id);
            return Result.Fail(ErrosDeEstabelecimento.CadastroDuplicado());
        }
    }
}
