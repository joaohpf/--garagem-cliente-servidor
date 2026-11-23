using Garagem.Application.Comum;
using Garagem.Application.Dtos;

namespace Garagem.Application.Servicos;

public interface IReservaService
{
    Task<IReadOnlyList<ReservaDto>> ListarAsync(Guid? veiculoId = null);
    Task<Resultado<ReservaDto>> ObterAsync(Guid id);
    Task<Resultado<ReservaDto>> ReservarAsync(CriarReservaDto dados);
    Task<Resultado<ReservaDto>> CancelarAsync(Guid id);
}
