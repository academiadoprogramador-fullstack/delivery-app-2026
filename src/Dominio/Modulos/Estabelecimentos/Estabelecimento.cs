using DeliveryApp.Dominio.Compartilhado;

namespace DeliveryApp.Dominio.Modulos.Estabelecimentos;

public sealed class Estabelecimento : EntidadeBase<Estabelecimento>
{
    public string NomeComercial { get; private set; } = string.Empty;
    public string NomeComercialNormalizado { get; private set; } = string.Empty;
    public string Documento { get; private set; } = string.Empty;
    public string Endereco { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;
    public TimeOnly HorarioAbertura { get; private set; }
    public TimeOnly HorarioFechamento { get; private set; }
    public string AreaAtendimento { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }

    private Estabelecimento() { }

    public Estabelecimento(
        Guid id,
        string nomeComercial,
        string documento,
        string endereco,
        string telefone,
        TimeOnly horarioAbertura,
        TimeOnly horarioFechamento,
        string areaAtendimento,
        bool ativo = true
    )
    {
        Id = id;
        DefinirDadosComerciais(
            nomeComercial,
            documento,
            endereco,
            telefone,
            horarioAbertura,
            horarioFechamento,
            areaAtendimento
        );
        Ativo = ativo;
    }

    public override IReadOnlyList<ErroValidacao> Validar()
    {
        List<ErroValidacao> erros = [];

        if (NomeComercial.Length is < 2 or > 100)
            erros.Add(new(nameof(NomeComercial), "O nome comercial deve possuir entre 2 e 100 caracteres."));

        if (Documento.Length is not 11 and not 14 || Documento.Any(c => !char.IsDigit(c)))
            erros.Add(new(nameof(Documento), "O documento deve possuir 11 ou 14 dígitos."));

        if (Endereco.Length is < 5 or > 250)
            erros.Add(new(nameof(Endereco), "O endereço deve possuir entre 5 e 250 caracteres."));

        if (Telefone.Length is not 10 and not 11 || Telefone.Any(c => !char.IsDigit(c)))
            erros.Add(new(nameof(Telefone), "O telefone deve possuir 10 ou 11 dígitos."));

        if (HorarioAbertura == HorarioFechamento)
            erros.Add(new(nameof(HorarioFechamento), "O horário de fechamento deve ser diferente do horário de abertura."));

        if (AreaAtendimento.Length is < 2 or > 500)
            erros.Add(new(nameof(AreaAtendimento), "A área de atendimento deve possuir entre 2 e 500 caracteres."));

        return erros;
    }

    public override void Atualizar(Estabelecimento entidadeAtualizada)
    {
        DefinirDadosComerciais(
            entidadeAtualizada.NomeComercial,
            entidadeAtualizada.Documento,
            entidadeAtualizada.Endereco,
            entidadeAtualizada.Telefone,
            entidadeAtualizada.HorarioAbertura,
            entidadeAtualizada.HorarioFechamento,
            entidadeAtualizada.AreaAtendimento
        );
    }

    public void Ativar() => Ativo = true;

    public void Desativar() => Ativo = false;

    public bool EstaDisponivelEm(TimeOnly horario)
    {
        if (!Ativo)
            return false;

        if (HorarioAbertura < HorarioFechamento)
            return horario >= HorarioAbertura && horario < HorarioFechamento;

        return horario >= HorarioAbertura || horario < HorarioFechamento;
    }

    public static string NormalizarNomeComercial(string nomeComercial)
    {
        return string.Join(' ', (nomeComercial ?? string.Empty)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
    }

    private void DefinirDadosComerciais(
        string nomeComercial,
        string documento,
        string endereco,
        string telefone,
        TimeOnly horarioAbertura,
        TimeOnly horarioFechamento,
        string areaAtendimento
    )
    {
        NomeComercial = NormalizarEspacos(nomeComercial);
        NomeComercialNormalizado = NormalizarNomeComercial(nomeComercial);
        Documento = RemoverMascara(documento);
        Endereco = NormalizarEspacos(endereco);
        Telefone = RemoverMascara(telefone);
        HorarioAbertura = horarioAbertura;
        HorarioFechamento = horarioFechamento;
        AreaAtendimento = NormalizarEspacos(areaAtendimento);
    }

    private static string NormalizarEspacos(string valor)
    {
        return string.Join(' ', (valor ?? string.Empty)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static string RemoverMascara(string valor)
    {
        return new string((valor ?? string.Empty)
            .Where(c => c is not '.' and not '/' and not '-' and not '(' and not ')' and not '+'
                && !char.IsWhiteSpace(c))
            .ToArray());
    }
}
