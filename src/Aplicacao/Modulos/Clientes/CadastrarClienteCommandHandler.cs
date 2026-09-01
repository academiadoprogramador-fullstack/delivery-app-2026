using DeliveryApp.Aplicacao.Compartilhado;
using DeliveryApp.Dominio.Modulos.Clientes;
using FluentResults;

namespace DeliveryApp.Aplicacao.Modulos.Clientes;

public sealed record CadastrarClienteCommand(Guid Id, string Nome, string Cpf);

public sealed class CadastrarClienteCommandHandler(IRepositorioCliente repositorioCliente)
{
    public async Task<Result> Handle(CadastrarClienteCommand command)
    {
        var cliente = new Cliente(
            command.Id,
            command.Nome,
            command.Cpf
        );

        var erros = cliente.Validar();

        if (erros.Count > 0)
        {
            return Result.Fail(
                new Error("Cliente inválido.")
                    .WithMetadata(nameof(TipoErro), TipoErro.Validacao)
            );
        }

        var clientes = await repositorioCliente.SelecionarTodosAsync();

        if (clientes.Any(registro => registro.Cpf == cliente.Cpf))
        {
            return Result.Fail(
                new Error("Um cliente com este CPF já foi cadastrado.")
                    .WithMetadata(nameof(TipoErro), TipoErro.Conflito)
            );
        }

        await repositorioCliente.CadastrarAsync(cliente);

        return Result.Ok();
    }
}
