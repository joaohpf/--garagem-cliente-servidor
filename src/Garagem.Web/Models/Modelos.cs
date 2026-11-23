using System.ComponentModel.DataAnnotations;

namespace Garagem.Web.Models;

// ---------- Dados recebidos da API (contrato JSON) ----------

public sealed record VeiculoModel(Guid Id, string Placa, string Modelo, int Ano, string Status);

public sealed record ReservaModel(
    Guid Id,
    Guid VeiculoId,
    string PlacaVeiculo,
    string ModeloVeiculo,
    string Cliente,
    DateOnly Inicio,
    DateOnly Fim,
    string Status,
    DateTimeOffset CriadaEm);

// ---------- Formulários (validação de formato no cliente) ----------
// As regras de negócio ficam no servidor; aqui só se verifica o preenchimento.

public sealed class VeiculoForm
{
    [Required(ErrorMessage = "Informe a placa.")]
    [StringLength(8, ErrorMessage = "Placa muito longa.")]
    public string Placa { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o modelo.")]
    public string Modelo { get; set; } = string.Empty;

    [Range(1950, 2100, ErrorMessage = "Ano inválido.")]
    public int Ano { get; set; } = DateTime.Today.Year;

    public string Status { get; set; } = "Disponivel";
}

public sealed class ReservaForm
{
    [Required(ErrorMessage = "Selecione um veículo.")]
    public Guid? VeiculoId { get; set; }

    [Required(ErrorMessage = "Informe o cliente.")]
    public string Cliente { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime Inicio { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime Fim { get; set; } = DateTime.Today.AddDays(1);
}

// ---------- ViewModels das telas ----------

public sealed class VeiculosPagina
{
    public IReadOnlyList<VeiculoModel> Veiculos { get; init; } = Array.Empty<VeiculoModel>();
    public VeiculoForm Form { get; init; } = new();
}

public sealed class ReservasPagina
{
    public IReadOnlyList<ReservaModel> Reservas { get; init; } = Array.Empty<ReservaModel>();
    public IReadOnlyList<VeiculoModel> Veiculos { get; init; } = Array.Empty<VeiculoModel>();
    public ReservaForm Form { get; init; } = new();
}

public static class StatusTexto
{
    public static string Veiculo(string status) => status switch
    {
        "Disponivel" => "Disponível",
        "EmManutencao" => "Em manutenção",
        "Inativo" => "Inativo",
        _ => status
    };
}
