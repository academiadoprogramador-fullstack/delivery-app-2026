namespace DeliveryApp.Aplicacao.Modulos.Estabelecimentos.DTOs;

public sealed record EstabelecimentoDto(
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

public sealed record EstabelecimentoCadastradoDto(Guid Id, string NomeComercial);
