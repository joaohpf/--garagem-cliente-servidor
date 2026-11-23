using Garagem.Domain.Entidades;
using Garagem.Domain.Repositorios;

namespace Garagem.Tests;

// Implementações em memória usadas nos testes. Graças à injeção de dependência,
// o serviço é testado sem arquivo, sem rede e sem a API.

internal sealed class VeiculoRepositoryFalso : IVeiculoRepository
{
    public List<Veiculo> Itens { get; } = new();

    public Task<IReadOnlyList<Veiculo>> ListarAsync() => Task.FromResult<IReadOnlyList<Veiculo>>(Itens.ToList());
    public Task<Veiculo?> ObterPorIdAsync(Guid id) => Task.FromResult(Itens.FirstOrDefault(v => v.Id == id));
    public Task<Veiculo?> ObterPorPlacaAsync(string placa) => Task.FromResult(Itens.FirstOrDefault(v => v.Placa == placa));
    public Task AdicionarAsync(Veiculo veiculo) { Itens.Add(veiculo); return Task.CompletedTask; }
    public Task AtualizarAsync(Veiculo veiculo) => Task.CompletedTask;
    public Task RemoverAsync(Guid id) { Itens.RemoveAll(v => v.Id == id); return Task.CompletedTask; }
}

internal sealed class ReservaRepositoryFalso : IReservaRepository
{
    public List<Reserva> Itens { get; } = new();

    public Task<IReadOnlyList<Reserva>> ListarAsync(Guid? veiculoId = null) =>
        Task.FromResult<IReadOnlyList<Reserva>>(Itens.Where(r => veiculoId is null || r.VeiculoId == veiculoId).ToList());
    public Task<Reserva?> ObterPorIdAsync(Guid id) => Task.FromResult(Itens.FirstOrDefault(r => r.Id == id));
    public Task AdicionarAsync(Reserva reserva) { Itens.Add(reserva); return Task.CompletedTask; }
    public Task AtualizarAsync(Reserva reserva) => Task.CompletedTask;
}

/// <summary>Relógio fixo, para que os testes não dependam da data de execução.</summary>
internal sealed class RelogioFixo : TimeProvider
{
    private readonly DateTimeOffset _agora;
    public RelogioFixo(DateTimeOffset agora) => _agora = agora;
    public override DateTimeOffset GetUtcNow() => _agora.ToUniversalTime();
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
