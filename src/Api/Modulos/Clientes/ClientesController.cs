using DeliveryApp.Dominio.Compartilhado.Auth;
using DeliveryApp.Dominio.Modulos.Clientes;
using DeliveryApp.Infraestrutura.Orm;
using DeliveryApp.WebApi.Compartilhado.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
    JwtProvider jwtProvider
) : ControllerBase
{

    [AllowAnonymous]
    [HttpPost("cadastro")]
    public async Task<ActionResult<AutenticacaoClienteResponse>> Cadastrar(
        CadastrarClienteRequest request
    )
    {
        var cliente = new Cliente(Guid.CreateVersion7(), request.Nome, request.Cpf);

        var erros = cliente.Validar();

        if (erros.Count > 0)
        {
            return BadRequest();
        }

        if (await dbContext.Clientes.AnyAsync(registro => registro.Cpf == cliente.Cpf))
        {
            return Conflict();
        }

        var usuario = new IdentityUser<Guid>
        {
            Id = cliente.Id,
            Email = request.Email.Trim(),
            UserName = request.Email.Trim()
        };

        try
        {
            var resultadoUsuario = await userManager.CreateAsync(usuario, request.Senha);

            if (!resultadoUsuario.Succeeded)
                return BadRequest();

            var tipoUsuario = TipoUsuario.Cliente.ToString();

            var resultadoRole = await roleManager.FindByNameAsync(tipoUsuario);

            if (resultadoRole is null)
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = new Guid("01a058f4-a048-79a3-b1a6-0f01d629a126"),
                    Name = TipoUsuario.Cliente.ToString(),
                    NormalizedName = TipoUsuario.Cliente.ToString().ToUpperInvariant(),
                    ConcurrencyStamp = "01a058f7-9492-73bc-8e4b-934c53594ed6"
                });
            }

            var resultadoInclusaoRole = await userManager.AddToRoleAsync(usuario, tipoUsuario);

            if (!resultadoInclusaoRole.Succeeded)
            {
                await userManager.DeleteAsync(usuario);

                return StatusCode(500);
            }

            dbContext.Clientes.Add(cliente);

            await dbContext.SaveChangesAsync();

            var jwt = jwtProvider.CriarToken(usuario.Id, usuario.Email!, TipoUsuario.Cliente);

            return StatusCode(StatusCodes.Status201Created, new AutenticacaoClienteResponse(
                usuario.Id,
                jwt.AccessToken,
                jwt.DataExpiracaoEmUtc
            ));
        }
        catch (DbUpdateException)
        {
            await userManager.DeleteAsync(usuario);

            return Conflict();
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
            return Unauthorized();

        var resultadoAutenticacao = await signInManager.CheckPasswordSignInAsync(
            usuario,
            request.Senha,
            lockoutOnFailure: true
        );

        if (!resultadoAutenticacao.Succeeded)
            return Unauthorized();

        var jwt = jwtProvider.CriarToken(usuario.Id, usuario.Email!, TipoUsuario.Cliente);

        return Ok(new AutenticacaoClienteResponse(
            usuario.Id,
            jwt.AccessToken,
            jwt.DataExpiracaoEmUtc
        ));
    }
}
