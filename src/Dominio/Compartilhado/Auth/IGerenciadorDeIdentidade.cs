namespace DeliveryApp.Dominio.Compartilhado.Auth;

public sealed class ConflitoDeIdentidadeException(string mensagem) : Exception(mensagem);

public sealed class ValidacaoDeIdentidadeException(
    string campo,
    string mensagem
) : Exception(mensagem)
{
    public string Campo { get; } = campo;
}

public sealed record UsuarioCadastrado(Guid Id, string Email);

public interface IGerenciadorDeIdentidade
{
    Task<UsuarioCadastrado> CadastrarAsync(
        Guid usuarioId,
        string email,
        string senha,
        TipoUsuario tipoUsuario
    );

    Task ExcluirAsync(Guid usuarioId);
}
