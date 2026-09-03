using DeliveryApp.Aplicacao.Compartilhado;
using DeliveryApp.Dominio.Compartilhado;
using FluentResults;

namespace DeliveryApp.Aplicacao.Modulos.Estabelecimentos;

public static class ErrosDeEstabelecimento
{
    public static Error NomeComercialDuplicado() =>
        Criar("Já existe um estabelecimento cadastrado com este nome comercial.", TipoErro.Conflito);

    public static Error CadastroDuplicado() =>
        Criar("Já existe um estabelecimento cadastrado com este nome comercial ou email.", TipoErro.Conflito);

    public static Error NaoEncontrado() =>
        Criar("O estabelecimento com este ID não foi encontrado.", TipoErro.NaoEncontrado);

    public static Error NaoAutorizado() =>
        Criar("Um estabelecimento pode alterar apenas suas próprias informações.", TipoErro.NaoAutorizado);

    public static Error CredenciaisInvalidas() =>
        Criar("O endereço de email ou senha informados são inválidos.", TipoErro.Validacao)
            .WithMetadata("Campo", "Credenciais");

    public static IEnumerable<Error> Validacao(IEnumerable<ErroValidacao> erros)
    {
        return erros.Select(erro => Criar(erro.Mensagem, TipoErro.Validacao)
            .WithMetadata("Campo", erro.Campo));
    }

    public static Error ConflitoDeIdentidade(string mensagem) => Criar(mensagem, TipoErro.Conflito);

    public static Error ValidacaoDeIdentidade(string campo, string mensagem) =>
        Criar(mensagem, TipoErro.Validacao).WithMetadata("Campo", campo);

    private static Error Criar(string mensagem, TipoErro tipo) =>
        new Error(mensagem).WithMetadata(nameof(TipoErro), tipo);
}
