using DeliveryApp.Dominio.Compartilhado.Auth;
using Microsoft.AspNetCore.Identity;

namespace DeliveryApp.Infraestrutura.Auth;

public sealed class GerenciadorDeIdentidade(
    UserManager<IdentityUser<Guid>> userManager
) : IGerenciadorDeIdentidade
{
    public async Task<UsuarioCadastrado> CadastrarAsync(
        Guid usuarioId,
        string email,
        string senha,
        TipoUsuario tipoUsuario
    )
    {
        IdentityUser<Guid> usuario = new()
        {
            Id = usuarioId,
            Email = email.Trim(),
            UserName = email.Trim()
        };

        IdentityResult resultadoUsuario = await userManager.CreateAsync(usuario, senha);

        if (!resultadoUsuario.Succeeded)
            throw CriarErro(resultadoUsuario);

        IdentityResult resultadoPapel = await userManager.AddToRoleAsync(
            usuario,
            tipoUsuario.ToString()
        );

        if (!resultadoPapel.Succeeded)
        {
            await userManager.DeleteAsync(usuario);
            throw CriarErro(resultadoPapel);
        }

        return new UsuarioCadastrado(usuario.Id, usuario.Email!);
    }

    public async Task ExcluirAsync(Guid usuarioId)
    {
        IdentityUser<Guid>? usuario = await userManager.FindByIdAsync(usuarioId.ToString());

        if (usuario is not null)
            await userManager.DeleteAsync(usuario);
    }

    private static Exception CriarErro(IdentityResult resultado)
    {
        if (resultado.Errors.Any(erro =>
            erro.Code is "DuplicateEmail" or "DuplicateUserName"
        ))
        {
            return new ConflitoDeIdentidadeException(
                "Já existe um usuário cadastrado com este email."
            );
        }

        IdentityError erro = resultado.Errors.First();
        string campo = erro.Code.StartsWith("Password", StringComparison.Ordinal)
            ? "Senha"
            : "Email";

        return new ValidacaoDeIdentidadeException(campo, erro.Description);
    }
}
