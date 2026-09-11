using DeliveryApp.Aplicacao.Modulos.Pedidos;
using DeliveryApp.Aplicacao.Modulos.Pedidos.DTOs;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.WebApi.Compartilhado.Http;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryApp.WebApi.Modulos.Pedidos;

[ApiController]
[Route("api/pedidos")]
[Authorize(Roles = nameof(TipoUsuario.Cliente) + "," + nameof(TipoUsuario.Estabelecimento))]
public sealed class PedidosController(IMediator mediator) : ControllerBase
{

    [HttpPost]
    [Authorize(Roles = nameof(TipoUsuario.Cliente))]
    [ProducesResponseType<CriarPedidoResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CriarPedidoResponse>> Criar(
        CriarPedidoRequest request,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new CriarPedidoCommand(
                request.EstabelecimentoId,
                request.EnderecoEntrega,
                request.Itens.Select(r => new ItemCriarPedidoCommand(
                    r.ProdutoId,
                    r.Quantidade,
                    r.Observacao,
                    r.ComplementosIds
                )).ToList()
            ),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return AcceptedAtAction(
            nameof(ObterPorId),
            new { pedidoId = resultado.Value },
            new CriarPedidoResponse(resultado.Value)
        );
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PedidoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PedidoDto>>> Listar(CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new ListarPedidosQuery(ObterTipoUsuario()),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return Ok(resultado.Value);
    }

    [HttpGet("{pedidoId:guid}")]
    [ProducesResponseType<PedidoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoDto>> ObterPorId(
        Guid pedidoId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new ObterPedidoQuery(pedidoId, ObterTipoUsuario()),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return Ok(resultado.Value);
    }

    private TipoUsuario ObterTipoUsuario()
    {
        return User.IsInRole(nameof(TipoUsuario.Estabelecimento))
            ? TipoUsuario.Estabelecimento
            : TipoUsuario.Cliente;
    }
}
