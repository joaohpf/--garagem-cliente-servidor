using Garagem.Domain.Entidades;

namespace Garagem.Domain.Repositorios;

public interface IReservaRepository
{
    Task<IReadOnlyList<Reserva>> ListarAsync(Guid? veiculoId = null);
    Task<Reserva?> ObterPorIdAsync(Guid id);
    Task AdicionarAsync(Reserva reserva);
    Task AtualizarAsync(Reserva reserva);
}
