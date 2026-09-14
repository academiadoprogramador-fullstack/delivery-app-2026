using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Pedidos;
using FluentResults;
using MediatR;

namespace DeliveryApp.Aplicacao.Modulos.Pedidos;

public sealed record AlterarStatusPedidoCommand(
    Guid PedidoId,
    TipoUsuario TipoUsuario,
    AcaoPedido Acao,
    string? Motivo
) : IRequest<Result<Guid>>;

