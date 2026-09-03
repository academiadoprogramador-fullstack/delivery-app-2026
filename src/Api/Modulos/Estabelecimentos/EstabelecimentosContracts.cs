namespace DeliveryApp.WebApi.Modulos.Estabelecimentos;

public sealed record CadastrarEstabelecimentoRequest(
    string NomeComercial,
    string Documento,
    string Endereco,
    string Telefone,
    string AreaAtendimento,
    TimeOnly HorarioAbertura,
    TimeOnly HorarioFechamento,
    string Email,
    string Senha
);

public sealed record CadastrarEstabelecimentoResponse(
    Guid EstabelecimentoId,
    string NomeComercial
);

