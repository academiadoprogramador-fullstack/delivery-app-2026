namespace DeliveryApp.WebApi.Modulos.Estabelecimentos;

public sealed record CadastrarEstabelecimentoRequest(
    string NomeComercial,
    string Documento,
    string Endereco,
    string Telefone,
    TimeOnly HorarioAbertura,
    TimeOnly HorarioFechamento,
    string AreaAtendimento,
    string Email,
    string Senha
);

public sealed record CadastrarEstabelecimentoResponse(Guid Id, string NomeComercial);

public sealed record AutenticarEstabelecimentoRequest(string Email, string Senha);

public sealed record AutenticarEstabelecimentoResponse(
    Guid EstabelecimentoId,
    string AccessToken,
    DateTime DataExpiracaoEmUtc
);

public sealed record EditarEstabelecimentoRequest(
    string NomeComercial,
    string Documento,
    string Endereco,
    string Telefone,
    TimeOnly HorarioAbertura,
    TimeOnly HorarioFechamento,
    string AreaAtendimento
);

public sealed record EstabelecimentoResponse(
    Guid Id,
    string NomeComercial,
    string Documento,
    string Endereco,
    string Telefone,
    TimeOnly HorarioAbertura,
    TimeOnly HorarioFechamento,
    string AreaAtendimento,
    bool Ativo
);
