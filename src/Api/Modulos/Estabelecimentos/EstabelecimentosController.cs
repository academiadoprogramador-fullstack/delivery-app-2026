using DeliveryApp.Aplicacao.Modulos.Estabelecimentos;
using DeliveryApp.WebApi.Compartilhado.Http;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryApp.WebApi.Modulos.Estabelecimentos;

[ApiController]
[Route("api/estabelecimentos")]
public sealed class EstabelecimentosController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("cadastro")]
    [ProducesResponseType<CadastrarEstabelecimentoResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CadastrarEstabelecimentoResponse>> Cadastrar(
       CadastrarEstabelecimentoRequest request,
       CancellationToken cancellationToken
   )
    {
        var resultado = await mediator.Send(new CadastrarEstabelecimentoCommand(
            request.NomeComercial,
            request.Documento,
            request.Endereco,
            request.Telefone,
            request.AreaAtendimento,
            request.HorarioAbertura,
            request.HorarioFechamento,
            request.Email,
            request.Senha
        ), cancellationToken);

        if (!resultado.IsSuccess)
            return this.ProblemDetails(resultado);

        return CreatedAtAction(
            string.Empty,
            new { clienteId = resultado.Value },
            new CadastrarEstabelecimentoResponse(
                resultado.Value,
                request.NomeComercial
            )
        );
    }
}
