using Garagem.Application.Comum;
using Garagem.Application.Dtos;
using Garagem.Domain;
using Garagem.Domain.Entidades;
using Garagem.Domain.Repositorios;

namespace Garagem.Application.Servicos;

/// <summary>
/// Casos de uso de veículos. Depende apenas de abstrações (repositórios e relógio),
/// recebidas por injeção de dependência no construtor.
/// </summary>
public sealed class VeiculoService : IVeiculoService
{
    private readonly IVeiculoRepository _veiculos;
    private readonly IReservaRepository _reservas;
    private readonly TimeProvider _relogio;

    public VeiculoService(IVeiculoRepository veiculos, IReservaRepository reservas, TimeProvider relogio)
    {
        _veiculos = veiculos;
        _reservas = reservas;
        _relogio = relogio;
    }

    private int AnoAtual => _relogio.GetLocalNow().Year;

    public async Task<IReadOnlyList<VeiculoDto>> ListarAsync() =>
        (await _veiculos.ListarAsync()).OrderBy(v => v.Modelo).Select(VeiculoDto.De).ToList();

    public async Task<Resultado<VeiculoDto>> ObterAsync(Guid id)
    {
        var veiculo = await _veiculos.ObterPorIdAsync(id);
        return veiculo is null
            ? Resultado<VeiculoDto>.Falha(TipoErro.NaoEncontrado, "Veículo não encontrado.")
            : Resultado<VeiculoDto>.Ok(VeiculoDto.De(veiculo));
    }

    public async Task<Resultado<VeiculoDto>> CadastrarAsync(SalvarVeiculoDto dados)
    {
        Veiculo veiculo;
        try
        {
            veiculo = Veiculo.Criar(dados.Placa, dados.Modelo, dados.Ano, AnoAtual);
            if (dados.Status != StatusVeiculo.Disponivel)
                veiculo.Atualizar(dados.Placa, dados.Modelo, dados.Ano, dados.Status, AnoAtual);
        }
        catch (RegraDeNegocioException ex)
        {
            return Resultado<VeiculoDto>.Falha(TipoErro.Validacao, ex.Message);
        }

        if (await _veiculos.ObterPorPlacaAsync(veiculo.Placa) is not null)
            return Resultado<VeiculoDto>.Falha(TipoErro.Conflito, $"Já existe um veículo com a placa {veiculo.Placa}.");

        await _veiculos.AdicionarAsync(veiculo);
        return Resultado<VeiculoDto>.Ok(VeiculoDto.De(veiculo));
    }

    public async Task<Resultado<VeiculoDto>> AtualizarAsync(Guid id, SalvarVeiculoDto dados)
    {
        var veiculo = await _veiculos.ObterPorIdAsync(id);
        if (veiculo is null)
            return Resultado<VeiculoDto>.Falha(TipoErro.NaoEncontrado, "Veículo não encontrado.");

        var mesmaPlaca = await _veiculos.ObterPorPlacaAsync(Veiculo.NormalizarPlaca(dados.Placa));
        if (mesmaPlaca is not null && mesmaPlaca.Id != id)
            return Resultado<VeiculoDto>.Falha(TipoErro.Conflito, $"Já existe um veículo com a placa {mesmaPlaca.Placa}.");

        try
        {
            veiculo.Atualizar(dados.Placa, dados.Modelo, dados.Ano, dados.Status, AnoAtual);
        }
        catch (RegraDeNegocioException ex)
        {
            return Resultado<VeiculoDto>.Falha(TipoErro.Validacao, ex.Message);
        }

        await _veiculos.AtualizarAsync(veiculo);
        return Resultado<VeiculoDto>.Ok(VeiculoDto.De(veiculo));
    }

    public async Task<Resultado> RemoverAsync(Guid id)
    {
        var veiculo = await _veiculos.ObterPorIdAsync(id);
        if (veiculo is null)
            return Resultado.Falha(TipoErro.NaoEncontrado, "Veículo não encontrado.");

        var possuiReservaAtiva = (await _reservas.ListarAsync(id)).Any(r => r.Status == StatusReserva.Ativa);
        if (possuiReservaAtiva)
            return Resultado.Falha(TipoErro.Conflito, "O veículo possui reservas ativas. Cancele-as antes de removê-lo.");

        await _veiculos.RemoverAsync(id);
        return Resultado.Ok();
    }
}
