using DeliveryApp.Aplicacao.Modulos.Clientes;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.WebApi.Compartilhado;
using DeliveryApp.WebApi.Compartilhado.Auth;
using DeliveryApp.WebApi.Compartilhado.Http;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryApp.WebApi.Modulos.Clientes;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController(
    UserManager<IdentityUser<Guid>> userManager,
    SignInManager<IdentityUser<Guid>> signInManager,
    IEmissorDeToken emissorTokens,
    IMediator mediator
) : ControllerBase
{

    [Authorize(Roles = nameof(TipoUsuario.Cliente))]
    [HttpGet("{clienteId:guid}")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ClienteResponse>> ObterPorId(
        Guid clienteId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(new ObterClientePorIdQuery(clienteId), cancellationToken);

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        var response = new ClienteResponse(
            resultado.Value.Id,
            resultado.Value.Nome,
            resultado.Value.Cpf,
            resultado.Value.Email
        );

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("cadastro")]
    [ProducesResponseType<CadastrarClienteResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CadastrarClienteResponse>> Cadastrar(
        CadastrarClienteRequest request,
        CancellationToken cancellationToken
    )
    {
        var resultadoCliente = await mediator.Send(new CadastrarClienteCommand(
            request.Nome,
            request.Cpf,
            request.Email,
            request.Senha
        ), cancellationToken);

        if (!resultadoCliente.IsSuccess)
            return this.ProblemDetails(resultadoCliente);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { clienteId = resultadoCliente.Value },
            new CadastrarClienteResponse(
                resultadoCliente.Value,
                request.Nome
            )
        );
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AutenticacaoClienteResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AutenticacaoClienteResponse>> Autenticar(
        AutenticarClienteRequest request
    )
    {
        var usuario = await userManager.FindByEmailAsync(request.Email.Trim());

        if (usuario is null)
            return this.CredenciaisInvalidas();

        var resultadoAutenticacao = await signInManager.CheckPasswordSignInAsync(
            usuario,
            request.Senha,
            lockoutOnFailure: true
        );

        if (!resultadoAutenticacao.Succeeded)
            return this.CredenciaisInvalidas();

        var accessToken = emissorTokens.CriarToken(usuario.Id, usuario.Email!, TipoUsuario.Cliente);

        return Ok(new AutenticacaoClienteResponse(
            usuario.Id,
            accessToken.Token,
            accessToken.DataExpiracaoEmUtc
        ));
    }
}
