using System.Data.Common;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Clientes;
using FluentResults;
using MediatR;

namespace DeliveryApp.Aplicacao.Modulos.Clientes;

public sealed record CadastrarClienteCommand(
    string Nome,
    string Cpf,
    string Email,
    string Senha
) : IRequest<Result<Guid>>;

public sealed class CadastrarClienteCommandHandler(
    IRepositorioCliente repositorio,
    IGerenciadorDeIdentidade gerenciadorDeIdentidade
) : IRequestHandler<CadastrarClienteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CadastrarClienteCommand command,
        CancellationToken cancellationToken = default
    )
    {
        Cliente cliente = new(
            Guid.CreateVersion7(),
            command.Nome,
            command.Cpf
        );

        var erros = cliente.Validar();

        if (erros.Count > 0)
            return Result.Fail(ErrosDeCliente.Validacao(erros));

        var clientes = await repositorio.SelecionarTodosAsync(cancellationToken);

        if (clientes.Any(registro => registro.Cpf == cliente.Cpf))
            return Result.Fail(ErrosDeCliente.CpfDuplicado());

        try
        {
            UsuarioCadastrado usuario = await gerenciadorDeIdentidade.CadastrarAsync(
                cliente.Id,
                command.Email,
                command.Senha,
                TipoUsuario.Cliente
            );

            await repositorio.CadastrarAsync(cliente, cancellationToken);

            return Result.Ok(cliente.Id);
        }
        catch (ConflitoDeIdentidadeException excecao)
        {
            return Result.Fail(
                ErrosDeCliente.ConflitoDeIdentidade(excecao.Message)
            );
        }
        catch (ValidacaoDeIdentidadeException excecao)
        {
            return Result.Fail(
                ErrosDeCliente.ValidacaoDeIdentidade(excecao.Campo, excecao.Message)
            );
        }
        catch (DbException)
        {
            await gerenciadorDeIdentidade.ExcluirAsync(cliente.Id);

            return Result.Fail(ErrosDeCliente.CadastroDuplicado());
        }
    }
}
