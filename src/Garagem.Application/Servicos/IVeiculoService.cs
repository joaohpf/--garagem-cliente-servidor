using Garagem.Application.Comum;
using Garagem.Application.Dtos;

namespace Garagem.Application.Servicos;

public interface IVeiculoService
{
    Task<IReadOnlyList<VeiculoDto>> ListarAsync();
    Task<Resultado<VeiculoDto>> ObterAsync(Guid id);
    Task<Resultado<VeiculoDto>> CadastrarAsync(SalvarVeiculoDto dados);
    Task<Resultado<VeiculoDto>> AtualizarAsync(Guid id, SalvarVeiculoDto dados);
    Task<Resultado> RemoverAsync(Guid id);
}
