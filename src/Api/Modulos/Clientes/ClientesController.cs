using DeliveryApp.Aplicacao.Modulos.Clientes;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.WebApi.Compartilhado;
using DeliveryApp.WebApi.Compartilhado.Auth;
using DeliveryApp.WebApi.Compartilhado.Http;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.WebApi.Modulos.Clientes;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController(
    UserManager<IdentityUser<Guid>> userManager,
    SignInManager<IdentityUser<Guid>> signInManager,
    IEmissorDeTokens emissorDeTokens,
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
    public async Task<ActionResult<CadastrarClienteResponse>> Cadastrar(
        CadastrarClienteRequest request,
        CancellationToken cancellationToken
    )
    {
        var id = Guid.CreateVersion7();

        var usuario = new IdentityUser<Guid>
        {
            Id = id,
            Email = request.Email.Trim(),
            UserName = request.Email.Trim()
        };

        try
        {
            var resultadoUsuario = await userManager.CreateAsync(usuario, request.Senha);

            if (!resultadoUsuario.Succeeded)
                return this.ErrosDeCriacaoUsuario(resultadoUsuario);

            var tipoUsuario = TipoUsuario.Cliente.ToString();

            var resultadoInclusaoPapel = await userManager.AddToRoleAsync(usuario, tipoUsuario);

            if (!resultadoInclusaoPapel.Succeeded)
            {
                await userManager.DeleteAsync(usuario);

                return this.ErrosDeCriacaoUsuario(resultadoInclusaoPapel);
            }

            var resultadoCliente = await mediator.Send(new CadastrarClienteCommand(
                id,
                request.Nome,
                request.Cpf
            ), cancellationToken);

            if (!resultadoCliente.IsSuccess)
                return this.ProblemDetails(resultadoCliente);

            return Created(string.Empty, new CadastrarClienteResponse(
                usuario.Id,
                request.Nome
            ));
        }
        catch (DbUpdateException)
        {
            await userManager.DeleteAsync(usuario);

            return this.Conflito(
                "Já existe um cliente cadastrado com este email ou CPF."
            );
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
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

        var accessToken = emissorDeTokens.CriarToken(usuario.Id, usuario.Email!, TipoUsuario.Cliente);

        return Ok(new AutenticacaoClienteResponse(
            usuario.Id,
            accessToken.Token,
            accessToken.DataExpiracaoEmUtc
        ));
    }
}
