using Garagem.Domain.Entidades;
using Garagem.Domain.Repositorios;
using Garagem.Infrastructure.Persistencia;

namespace Garagem.Infrastructure.Repositorios;

/// <summary>Implementação de IReservaRepository sobre arquivo JSON.</summary>
public sealed class JsonReservaRepository : IReservaRepository
{
    private readonly ArquivoJson<ReservaRegistro> _arquivo;

    public JsonReservaRepository(ArquivoJson<ReservaRegistro> arquivo)
    {
        _arquivo = arquivo;
    }

    public async Task<IReadOnlyList<Reserva>> ListarAsync(Guid? veiculoId = null) =>
        (await _arquivo.LerAsync())
            .Where(r => veiculoId is null || r.VeiculoId == veiculoId)
            .Select(r => r.ParaDominio())
            .ToList();

    public async Task<Reserva?> ObterPorIdAsync(Guid id) =>
        (await _arquivo.LerAsync()).FirstOrDefault(r => r.Id == id)?.ParaDominio();

    public Task AdicionarAsync(Reserva reserva) =>
        _arquivo.AlterarAsync(lista => lista.Add(ReservaRegistro.De(reserva)));

    public Task AtualizarAsync(Reserva reserva) =>
        _arquivo.AlterarAsync(lista =>
        {
            var indice = lista.FindIndex(r => r.Id == reserva.Id);
            if (indice >= 0)
                lista[indice] = ReservaRegistro.De(reserva);
        });
}
