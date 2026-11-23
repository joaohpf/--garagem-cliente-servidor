namespace Garagem.Domain.Entidades;

public enum StatusReserva
{
    Ativa = 0,
    Cancelada = 1
}

/// <summary>
/// Reserva de um veículo para um período fechado [Inicio, Fim] (as duas datas contam).
/// </summary>
public sealed class Reserva
{
    public Guid Id { get; }
    public Guid VeiculoId { get; private set; }
    public string Cliente { get; private set; } = string.Empty;
    public DateOnly Inicio { get; private set; }
    public DateOnly Fim { get; private set; }
    public StatusReserva Status { get; private set; }
    public DateTimeOffset CriadaEm { get; private set; }

    private Reserva(Guid id)
    {
        Id = id;
    }

    public static Reserva Criar(Guid veiculoId, string cliente, DateOnly inicio, DateOnly fim, DateTimeOffset agora)
    {
        if (veiculoId == Guid.Empty)
            throw new RegraDeNegocioException("O veículo é obrigatório.");

        if (string.IsNullOrWhiteSpace(cliente))
            throw new RegraDeNegocioException("O nome do cliente é obrigatório.");

        if (fim < inicio)
            throw new RegraDeNegocioException("A data final não pode ser anterior à data inicial.");

        if (inicio < DateOnly.FromDateTime(agora.Date))
            throw new RegraDeNegocioException("Não é possível reservar para uma data passada.");

        return new Reserva(Guid.NewGuid())
        {
            VeiculoId = veiculoId,
            Cliente = cliente.Trim(),
            Inicio = inicio,
            Fim = fim,
            Status = StatusReserva.Ativa,
            CriadaEm = agora
        };
    }

    /// <summary>Reconstrói uma reserva já persistida (usado pela infraestrutura).</summary>
    public static Reserva Restaurar(Guid id, Guid veiculoId, string cliente, DateOnly inicio, DateOnly fim,
        StatusReserva status, DateTimeOffset criadaEm) =>
        new(id)
        {
            VeiculoId = veiculoId,
            Cliente = cliente,
            Inicio = inicio,
            Fim = fim,
            Status = status,
            CriadaEm = criadaEm
        };

    /// <summary>Dois períodos fechados se sobrepõem quando cada um começa antes do fim do outro.</summary>
    public bool ConflitaCom(DateOnly inicio, DateOnly fim) =>
        Status == StatusReserva.Ativa && inicio <= Fim && fim >= Inicio;

    public void Cancelar()
    {
        if (Status == StatusReserva.Cancelada)
            throw new RegraDeNegocioException("A reserva já está cancelada.");

        Status = StatusReserva.Cancelada;
    }
}
