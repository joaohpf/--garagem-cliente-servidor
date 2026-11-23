using Garagem.Domain.Entidades;

namespace Garagem.Application.Dtos;

public sealed record CriarReservaDto(Guid VeiculoId, string Cliente, DateOnly Inicio, DateOnly Fim);

public sealed record ReservaDto(
    Guid Id,
    Guid VeiculoId,
    string PlacaVeiculo,
    string ModeloVeiculo,
    string Cliente,
    DateOnly Inicio,
    DateOnly Fim,
    StatusReserva Status,
    DateTimeOffset CriadaEm)
{
    public static ReservaDto De(Reserva r, Veiculo? v) => new(
        r.Id, r.VeiculoId, v?.Placa ?? "(removido)", v?.Modelo ?? "-", r.Cliente, r.Inicio, r.Fim, r.Status, r.CriadaEm);
}
