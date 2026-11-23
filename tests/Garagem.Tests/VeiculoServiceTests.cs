using Garagem.Application.Comum;
using Garagem.Application.Dtos;
using Garagem.Application.Servicos;
using Garagem.Domain.Entidades;

namespace Garagem.Tests;

public class VeiculoServiceTests
{
    private readonly VeiculoRepositoryFalso _veiculos = new();
    private readonly ReservaRepositoryFalso _reservas = new();
    private readonly VeiculoService _servico;

    public VeiculoServiceTests()
    {
        var relogio = new RelogioFixo(new DateTimeOffset(2026, 11, 23, 10, 0, 0, TimeSpan.Zero));
        _servico = new VeiculoService(_veiculos, _reservas, relogio);
    }

    [Fact]
    public async Task Cadastro_normaliza_a_placa()
    {
        var resultado = await _servico.CadastrarAsync(new SalvarVeiculoDto("abc-1d23", "Fiat Argo", 2022));

        Assert.True(resultado.Sucesso);
        Assert.Equal("ABC1D23", resultado.Valor!.Placa);
    }

    [Fact]
    public async Task Placa_duplicada_retorna_conflito()
    {
        await _servico.CadastrarAsync(new SalvarVeiculoDto("ABC1D23", "Fiat Argo", 2022));

        var resultado = await _servico.CadastrarAsync(new SalvarVeiculoDto("ABC1D23", "Outro", 2020));

        Assert.Equal(TipoErro.Conflito, resultado.Erro);
        Assert.Single(_veiculos.Itens);
    }

    [Theory]
    [InlineData("12345", "Fiat Argo", 2022)]
    [InlineData("ABC1D23", "", 2022)]
    [InlineData("ABC1D23", "Fiat Argo", 1900)]
    [InlineData("ABC1D23", "Fiat Argo", 2030)]
    public async Task Dados_invalidos_retornam_validacao(string placa, string modelo, int ano)
    {
        var resultado = await _servico.CadastrarAsync(new SalvarVeiculoDto(placa, modelo, ano));

        Assert.Equal(TipoErro.Validacao, resultado.Erro);
        Assert.Empty(_veiculos.Itens);
    }

    [Fact]
    public async Task Veiculo_com_reserva_ativa_nao_pode_ser_removido()
    {
        var veiculo = (await _servico.CadastrarAsync(new SalvarVeiculoDto("ABC1D23", "Fiat Argo", 2022))).Valor!;
        _reservas.Itens.Add(Reserva.Criar(veiculo.Id, "Cliente", new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 2),
            new DateTimeOffset(2026, 11, 23, 10, 0, 0, TimeSpan.Zero)));

        var resultado = await _servico.RemoverAsync(veiculo.Id);

        Assert.Equal(TipoErro.Conflito, resultado.Erro);
        Assert.Single(_veiculos.Itens);
    }

    [Fact]
    public async Task Veiculo_sem_reserva_ativa_e_removido()
    {
        var veiculo = (await _servico.CadastrarAsync(new SalvarVeiculoDto("ABC1D23", "Fiat Argo", 2022))).Valor!;

        var resultado = await _servico.RemoverAsync(veiculo.Id);

        Assert.True(resultado.Sucesso);
        Assert.Empty(_veiculos.Itens);
    }
}
