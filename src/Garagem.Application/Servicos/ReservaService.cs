using Garagem.Application.Comum;
using Garagem.Application.Dtos;
using Garagem.Domain;
using Garagem.Domain.Entidades;
using Garagem.Domain.Repositorios;

namespace Garagem.Application.Servicos;

/// <summary>
/// Caso de uso "reservar veículo" (seção 6.3 da parte teórica):
/// localiza o veículo, verifica disponibilidade e conflitos e grava a reserva.
/// </summary>
public sealed class ReservaService : IReservaService
{
    // Serializa "verificar conflito + gravar" para que duas requisições simultâneas
    // não reservem o mesmo veículo no mesmo período (instância única da API).
    private static readonly SemaphoreSlim TravaReserva = new(1, 1);

    private readonly IReservaRepository _reservas;
    private readonly IVeiculoRepository _veiculos;
    private readonly TimeProvider _relogio;

    public ReservaService(IReservaRepository reservas, IVeiculoRepository veiculos, TimeProvider relogio)
    {
        _reservas = reservas;
        _veiculos = veiculos;
        _relogio = relogio;
    }

    public async Task<IReadOnlyList<ReservaDto>> ListarAsync(Guid? veiculoId = null)
    {
        var reservas = await _reservas.ListarAsync(veiculoId);
        var veiculos = (await _veiculos.ListarAsync()).ToDictionary(v => v.Id);

        return reservas
            .OrderBy(r => r.Status)
            .ThenBy(r => r.Inicio)
            .Select(r => ReservaDto.De(r, veiculos.GetValueOrDefault(r.VeiculoId)))
            .ToList();
    }

    public async Task<Resultado<ReservaDto>> ObterAsync(Guid id)
    {
        var reserva = await _reservas.ObterPorIdAsync(id);
        if (reserva is null)
            return Resultado<ReservaDto>.Falha(TipoErro.NaoEncontrado, "Reserva não encontrada.");

        var veiculo = await _veiculos.ObterPorIdAsync(reserva.VeiculoId);
        return Resultado<ReservaDto>.Ok(ReservaDto.De(reserva, veiculo));
    }

    public async Task<Resultado<ReservaDto>> ReservarAsync(CriarReservaDto dados)
    {
        await TravaReserva.WaitAsync();
        try
        {
            var veiculo = await _veiculos.ObterPorIdAsync(dados.VeiculoId);
            if (veiculo is null)
                return Resultado<ReservaDto>.Falha(TipoErro.NaoEncontrado, "Veículo não encontrado.");

            if (!veiculo.PodeSerReservado)
                return Resultado<ReservaDto>.Falha(TipoErro.Conflito,
                    $"O veículo {veiculo.Placa} não está disponível para reserva (status: {veiculo.Status}).");

            Reserva reserva;
            try
            {
                reserva = Reserva.Criar(dados.VeiculoId, dados.Cliente, dados.Inicio, dados.Fim, _relogio.GetLocalNow());
            }
            catch (RegraDeNegocioException ex)
            {
                return Resultado<ReservaDto>.Falha(TipoErro.Validacao, ex.Message);
            }

            var conflito = (await _reservas.ListarAsync(veiculo.Id))
                .FirstOrDefault(r => r.ConflitaCom(reserva.Inicio, reserva.Fim));
            if (conflito is not null)
                return Resultado<ReservaDto>.Falha(TipoErro.Conflito,
                    $"O veículo {veiculo.Placa} já está reservado de {conflito.Inicio:dd/MM/yyyy} a {conflito.Fim:dd/MM/yyyy}.");

            await _reservas.AdicionarAsync(reserva);
            return Resultado<ReservaDto>.Ok(ReservaDto.De(reserva, veiculo));
        }
        finally
        {
            TravaReserva.Release();
        }
    }

    public async Task<Resultado<ReservaDto>> CancelarAsync(Guid id)
    {
        var reserva = await _reservas.ObterPorIdAsync(id);
        if (reserva is null)
            return Resultado<ReservaDto>.Falha(TipoErro.NaoEncontrado, "Reserva não encontrada.");

        try
        {
            reserva.Cancelar();
        }
        catch (RegraDeNegocioException ex)
        {
            return Resultado<ReservaDto>.Falha(TipoErro.Conflito, ex.Message);
        }

        await _reservas.AtualizarAsync(reserva);
        var veiculo = await _veiculos.ObterPorIdAsync(reserva.VeiculoId);
        return Resultado<ReservaDto>.Ok(ReservaDto.De(reserva, veiculo));
    }
}
