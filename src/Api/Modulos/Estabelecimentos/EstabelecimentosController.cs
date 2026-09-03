using DeliveryApp.Aplicacao.Modulos.Estabelecimentos;
using DeliveryApp.Aplicacao.Modulos.Estabelecimentos.DTOs;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.WebApi.Compartilhado.Http;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryApp.WebApi.Modulos.Estabelecimentos;

[ApiController]
[Route("api/estabelecimentos")]
public sealed class EstabelecimentosController(IMediator mediator) : ControllerBase
{
    private const string TiposAutorizados = "Cliente,Estabelecimento";

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
            request.HorarioAbertura,
            request.HorarioFechamento,
            request.AreaAtendimento,
            request.Email,
            request.Senha
        ), cancellationToken);

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { estabelecimentoId = resultado.Value.Id },
            new CadastrarEstabelecimentoResponse(
                resultado.Value.Id,
                resultado.Value.NomeComercial
            )
        );
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AutenticarEstabelecimentoResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AutenticarEstabelecimentoResponse>> Autenticar(
        AutenticarEstabelecimentoRequest request,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new AutenticarEstabelecimentoCommand(request.Email, request.Senha),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return Ok(new AutenticarEstabelecimentoResponse(
            resultado.Value.EstabelecimentoId,
            resultado.Value.Token,
            resultado.Value.DataExpiracaoEmUtc
        ));
    }

    [Authorize(Roles = TiposAutorizados)]
    [HttpGet]
    [ProducesResponseType<List<EstabelecimentoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EstabelecimentoResponse>>> Listar(
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(new ListarEstabelecimentosQuery(), cancellationToken);
        return Ok(resultado.Value.Select(Mapear).ToList());
    }

    [Authorize(Roles = TiposAutorizados)]
    [HttpGet("disponiveis")]
    [ProducesResponseType<List<EstabelecimentoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EstabelecimentoResponse>>> ListarDisponiveis(
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new ListarEstabelecimentosDisponiveisQuery(),
            cancellationToken
        );

        return Ok(resultado.Value.Select(Mapear).ToList());
    }

    [Authorize(Roles = TiposAutorizados)]
    [HttpGet("{estabelecimentoId:guid}")]
    [ProducesResponseType<EstabelecimentoResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EstabelecimentoResponse>> ObterPorId(
        Guid estabelecimentoId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new ObterEstabelecimentoPorIdQuery(estabelecimentoId),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return Ok(Mapear(resultado.Value));
    }

    [Authorize(Roles = nameof(TipoUsuario.Estabelecimento))]
    [HttpPut("{estabelecimentoId:guid}")]
    [ProducesResponseType<EstabelecimentoResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EstabelecimentoResponse>> Editar(
        Guid estabelecimentoId,
        EditarEstabelecimentoRequest request,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(new EditarEstabelecimentoCommand(
            estabelecimentoId,
            request.NomeComercial,
            request.Documento,
            request.Endereco,
            request.Telefone,
            request.HorarioAbertura,
            request.HorarioFechamento,
            request.AreaAtendimento
        ), cancellationToken);

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return Ok(Mapear(resultado.Value));
    }

    [Authorize(Roles = nameof(TipoUsuario.Estabelecimento))]
    [HttpPatch("{estabelecimentoId:guid}/ativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Ativar(
        Guid estabelecimentoId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new AtivarEstabelecimentoCommand(estabelecimentoId),
            cancellationToken
        );

        return resultado.IsFailed ? this.ProblemDetails(resultado) : NoContent();
    }

    [Authorize(Roles = nameof(TipoUsuario.Estabelecimento))]
    [HttpPatch("{estabelecimentoId:guid}/desativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desativar(
        Guid estabelecimentoId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new DesativarEstabelecimentoCommand(estabelecimentoId),
            cancellationToken
        );

        return resultado.IsFailed ? this.ProblemDetails(resultado) : NoContent();
    }

    private static EstabelecimentoResponse Mapear(EstabelecimentoDto estabelecimento) => new(
        estabelecimento.Id,
        estabelecimento.NomeComercial,
        estabelecimento.Documento,
        estabelecimento.Endereco,
        estabelecimento.Telefone,
        estabelecimento.HorarioAbertura,
        estabelecimento.HorarioFechamento,
        estabelecimento.AreaAtendimento,
        estabelecimento.Ativo
    );
}
