using Garagem.Application.Comum;
using Garagem.Application.Dtos;
using Garagem.Application.Servicos;
using Garagem.Domain.Entidades;

namespace Garagem.Tests;

public class ReservaServiceTests
{
    private static readonly DateOnly Hoje = new(2026, 11, 23);

    private readonly VeiculoRepositoryFalso _veiculos = new();
    private readonly ReservaRepositoryFalso _reservas = new();
    private readonly ReservaService _servico;
    private readonly Veiculo _argo;

    public ReservaServiceTests()
    {
        var relogio = new RelogioFixo(new DateTimeOffset(2026, 11, 23, 10, 0, 0, TimeSpan.Zero));
        _servico = new ReservaService(_reservas, _veiculos, relogio);

        _argo = Veiculo.Criar("ABC1D23", "Fiat Argo", 2022, 2026);
        _veiculos.Itens.Add(_argo);
    }

    private Task<Resultado<ReservaDto>> Reservar(int diaInicio, int diaFim, Guid? veiculoId = null) =>
        _servico.ReservarAsync(new CriarReservaDto(
            veiculoId ?? _argo.Id, "Cliente Teste", Hoje.AddDays(diaInicio), Hoje.AddDays(diaFim)));

    [Fact]
    public async Task Reserva_valida_e_gravada()
    {
        var resultado = await Reservar(1, 3);

        Assert.True(resultado.Sucesso);
        Assert.Single(_reservas.Itens);
        Assert.Equal("ABC1D23", resultado.Valor!.PlacaVeiculo);
    }

    [Fact]
    public async Task Reserva_com_periodo_conflitante_e_rejeitada()
    {
        await Reservar(1, 5);

        var resultado = await Reservar(4, 8);

        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErro.Conflito, resultado.Erro);
        Assert.Single(_reservas.Itens);
    }

    [Fact]
    public async Task Data_final_e_inclusiva_no_conflito()
    {
        await Reservar(1, 5);

        var resultado = await Reservar(5, 6);

        Assert.Equal(TipoErro.Conflito, resultado.Erro);
    }

    [Fact]
    public async Task Periodos_vizinhos_sem_sobreposicao_sao_aceitos()
    {
        await Reservar(1, 5);

        var resultado = await Reservar(6, 7);

        Assert.True(resultado.Sucesso);
        Assert.Equal(2, _reservas.Itens.Count);
    }

    [Fact]
    public async Task Reserva_cancelada_libera_o_periodo()
    {
        var primeira = await Reservar(1, 5);
        await _servico.CancelarAsync(primeira.Valor!.Id);

        var resultado = await Reservar(2, 3);

        Assert.True(resultado.Sucesso);
    }

    [Fact]
    public async Task Veiculo_em_manutencao_nao_pode_ser_reservado()
    {
        _argo.Atualizar(_argo.Placa, _argo.Modelo, _argo.Ano, StatusVeiculo.EmManutencao, 2026);

        var resultado = await Reservar(1, 2);

        Assert.Equal(TipoErro.Conflito, resultado.Erro);
        Assert.Empty(_reservas.Itens);
    }

    [Fact]
    public async Task Veiculo_inexistente_retorna_nao_encontrado()
    {
        var resultado = await Reservar(1, 2, Guid.NewGuid());

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Erro);
    }

    [Fact]
    public async Task Data_final_anterior_a_inicial_e_invalida()
    {
        var resultado = await Reservar(5, 2);

        Assert.Equal(TipoErro.Validacao, resultado.Erro);
    }

    [Fact]
    public async Task Reserva_no_passado_e_invalida()
    {
        var resultado = await Reservar(-2, 1);

        Assert.Equal(TipoErro.Validacao, resultado.Erro);
    }

    [Fact]
    public async Task Cancelar_duas_vezes_retorna_conflito()
    {
        var reserva = await Reservar(1, 2);
        await _servico.CancelarAsync(reserva.Valor!.Id);

        var resultado = await _servico.CancelarAsync(reserva.Valor!.Id);

        Assert.Equal(TipoErro.Conflito, resultado.Erro);
    }

    [Fact]
    public async Task Requisicoes_simultaneas_nao_geram_reserva_duplicada()
    {
        var tarefas = Enumerable.Range(0, 20).Select(_ => Task.Run(() => Reservar(1, 3)));

        var resultados = await Task.WhenAll(tarefas);

        Assert.Equal(1, resultados.Count(r => r.Sucesso));
        Assert.Single(_reservas.Itens);
    }
}
