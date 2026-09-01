using DeliveryApp.Aplicacao.Modulos.Clientes;
using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Clientes;
using DeliveryApp.Infraestrutura.Orm;
using DeliveryApp.WebApi.Compartilhado;
using DeliveryApp.WebApi.Compartilhado.Auth;
using DeliveryApp.WebApi.Compartilhado.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.WebApi.Modulos.Clientes;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController(
    DeliveryAppDbContext dbContext,
    UserManager<IdentityUser<Guid>> userManager,
    SignInManager<IdentityUser<Guid>> signInManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    JwtProvider jwtProvider,
    ObterClientePorIdQueryHandler obterClientePorIdQueryHandler,
    CadastrarClienteCommandHandler cadastrarClienteCommandHandler
) : ControllerBase
{

    [Authorize(Roles = nameof(TipoUsuario.Cliente))]
    [HttpGet("{clienteId:guid}")]
    public async Task<ActionResult<ClienteResponse>> ObterPorId(
        Guid clienteId
    )
    {
        var resultado = await obterClientePorIdQueryHandler.Handle(new ObterClientePorIdQuery(clienteId));

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
    public async Task<ActionResult<AutenticacaoClienteResponse>> Cadastrar(
        CadastrarClienteRequest request
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

            var resultadoPapel = await roleManager.FindByNameAsync(tipoUsuario);

            if (resultadoPapel is null)
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = new Guid("01a058f4-a048-79a3-b1a6-0f01d629a126"),
                    Name = TipoUsuario.Cliente.ToString(),
                    NormalizedName = TipoUsuario.Cliente.ToString().ToUpperInvariant(),
                    ConcurrencyStamp = "01a058f7-9492-73bc-8e4b-934c53594ed6"
                });
            }

            var resultadoInclusaoPapel = await userManager.AddToRoleAsync(usuario, tipoUsuario);

            if (!resultadoInclusaoPapel.Succeeded)
            {
                await userManager.DeleteAsync(usuario);

                return this.ErrosDeCriacaoUsuario(resultadoInclusaoPapel);
            }

            await cadastrarClienteCommandHandler.Handle(new CadastrarClienteCommand(
                id,
                request.Nome,
                request.Cpf
            ));

            var jwt = jwtProvider.CriarToken(usuario.Id, usuario.Email!, TipoUsuario.Cliente);

            return Created(string.Empty, new AutenticacaoClienteResponse(
                usuario.Id,
                jwt.AccessToken,
                jwt.DataExpiracaoEmUtc
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

        var jwt = jwtProvider.CriarToken(usuario.Id, usuario.Email!, TipoUsuario.Cliente);

        return Ok(new AutenticacaoClienteResponse(
            usuario.Id,
            jwt.AccessToken,
            jwt.DataExpiracaoEmUtc
        ));
    }
}
