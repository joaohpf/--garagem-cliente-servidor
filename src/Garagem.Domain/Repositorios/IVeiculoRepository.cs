using Garagem.Domain.Entidades;

namespace Garagem.Domain.Repositorios;

/// <summary>
/// Contrato definido pelo núcleo e implementado pela infraestrutura
/// (inversão de dependência: o domínio não referencia a infraestrutura).
/// </summary>
public interface IVeiculoRepository
{
    Task<IReadOnlyList<Veiculo>> ListarAsync();
    Task<Veiculo?> ObterPorIdAsync(Guid id);
    Task<Veiculo?> ObterPorPlacaAsync(string placa);
    Task AdicionarAsync(Veiculo veiculo);
    Task AtualizarAsync(Veiculo veiculo);
    Task RemoverAsync(Guid id);
}
