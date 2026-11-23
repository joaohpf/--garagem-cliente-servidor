using Garagem.Domain.Entidades;
using Garagem.Domain.Repositorios;
using Garagem.Infrastructure.Persistencia;

namespace Garagem.Infrastructure.Repositorios;

/// <summary>Implementação de IVeiculoRepository sobre arquivo JSON.</summary>
public sealed class JsonVeiculoRepository : IVeiculoRepository
{
    private readonly ArquivoJson<VeiculoRegistro> _arquivo;

    public JsonVeiculoRepository(ArquivoJson<VeiculoRegistro> arquivo)
    {
        _arquivo = arquivo;
    }

    public async Task<IReadOnlyList<Veiculo>> ListarAsync() =>
        (await _arquivo.LerAsync()).Select(r => r.ParaDominio()).ToList();

    public async Task<Veiculo?> ObterPorIdAsync(Guid id) =>
        (await _arquivo.LerAsync()).FirstOrDefault(r => r.Id == id)?.ParaDominio();

    public async Task<Veiculo?> ObterPorPlacaAsync(string placa) =>
        (await _arquivo.LerAsync())
            .FirstOrDefault(r => string.Equals(r.Placa, placa, StringComparison.OrdinalIgnoreCase))?
            .ParaDominio();

    public Task AdicionarAsync(Veiculo veiculo) =>
        _arquivo.AlterarAsync(lista => lista.Add(VeiculoRegistro.De(veiculo)));

    public Task AtualizarAsync(Veiculo veiculo) =>
        _arquivo.AlterarAsync(lista =>
        {
            var indice = lista.FindIndex(r => r.Id == veiculo.Id);
            if (indice >= 0)
                lista[indice] = VeiculoRegistro.De(veiculo);
        });

    public Task RemoverAsync(Guid id) =>
        _arquivo.AlterarAsync(lista => lista.RemoveAll(r => r.Id == id));
}
