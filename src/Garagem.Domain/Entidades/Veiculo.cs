using System.Text.RegularExpressions;

namespace Garagem.Domain.Entidades;

public enum StatusVeiculo
{
    Disponivel = 0,
    EmManutencao = 1,
    Inativo = 2
}

/// <summary>
/// Entidade de domínio. Não conhece HTTP, JSON nem banco de dados.
/// </summary>
public sealed class Veiculo
{
    // Placa no padrão antigo (ABC1234) ou Mercosul (ABC1D23).
    private static readonly Regex FormatoPlaca = new("^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", RegexOptions.Compiled);
    private const int AnoMinimo = 1950;

    public Guid Id { get; }
    public string Placa { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public int Ano { get; private set; }
    public StatusVeiculo Status { get; private set; }

    private Veiculo(Guid id)
    {
        Id = id;
    }

    public static Veiculo Criar(string placa, string modelo, int ano, int anoAtual)
    {
        var veiculo = new Veiculo(Guid.NewGuid()) { Status = StatusVeiculo.Disponivel };
        veiculo.Atualizar(placa, modelo, ano, StatusVeiculo.Disponivel, anoAtual);
        return veiculo;
    }

    /// <summary>Reconstrói um veículo já persistido (usado pela infraestrutura).</summary>
    public static Veiculo Restaurar(Guid id, string placa, string modelo, int ano, StatusVeiculo status) =>
        new(id) { Placa = placa, Modelo = modelo, Ano = ano, Status = status };

    public void Atualizar(string placa, string modelo, int ano, StatusVeiculo status, int anoAtual)
    {
        var placaNormalizada = NormalizarPlaca(placa);
        if (!FormatoPlaca.IsMatch(placaNormalizada))
            throw new RegraDeNegocioException("Placa inválida. Use o formato ABC1234 ou ABC1D23.");

        if (string.IsNullOrWhiteSpace(modelo))
            throw new RegraDeNegocioException("O modelo é obrigatório.");

        if (ano < AnoMinimo || ano > anoAtual + 1)
            throw new RegraDeNegocioException($"O ano deve estar entre {AnoMinimo} e {anoAtual + 1}.");

        if (!Enum.IsDefined(status))
            throw new RegraDeNegocioException("Status de veículo inválido.");

        Placa = placaNormalizada;
        Modelo = modelo.Trim();
        Ano = ano;
        Status = status;
    }

    public bool PodeSerReservado => Status == StatusVeiculo.Disponivel;

    public static string NormalizarPlaca(string? placa) =>
        (placa ?? string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
}
