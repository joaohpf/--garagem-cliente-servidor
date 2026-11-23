using Garagem.Domain.Entidades;

namespace Garagem.Infrastructure.Persistencia;

// Modelos de persistência: refletem o formato gravado no arquivo.
// Ficam na infraestrutura para que o domínio não dependa do formato de armazenamento.

public sealed class VeiculoRegistro
{
    public Guid Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public StatusVeiculo Status { get; set; }

    public static VeiculoRegistro De(Veiculo v) =>
        new() { Id = v.Id, Placa = v.Placa, Modelo = v.Modelo, Ano = v.Ano, Status = v.Status };

    public Veiculo ParaDominio() => Veiculo.Restaurar(Id, Placa, Modelo, Ano, Status);
}

public sealed class ReservaRegistro
{
    public Guid Id { get; set; }
    public Guid VeiculoId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public DateOnly Inicio { get; set; }
    public DateOnly Fim { get; set; }
    public StatusReserva Status { get; set; }
    public DateTimeOffset CriadaEm { get; set; }

    public static ReservaRegistro De(Reserva r) => new()
    {
        Id = r.Id,
        VeiculoId = r.VeiculoId,
        Cliente = r.Cliente,
        Inicio = r.Inicio,
        Fim = r.Fim,
        Status = r.Status,
        CriadaEm = r.CriadaEm
    };

    public Reserva ParaDominio() => Reserva.Restaurar(Id, VeiculoId, Cliente, Inicio, Fim, Status, CriadaEm);
}
