using Garagem.Domain.Entidades;

namespace Garagem.Infrastructure.Persistencia;

/// <summary>Cria alguns veículos na primeira execução, para facilitar a demonstração.</summary>
public static class DadosIniciais
{
    public static async Task CarregarAsync(ArquivoJson<VeiculoRegistro> veiculos)
    {
        if (veiculos.Existe)
            return;

        var iniciais = new[]
        {
            Veiculo.Restaurar(Guid.NewGuid(), "ABC1D23", "Fiat Argo", 2022, StatusVeiculo.Disponivel),
            Veiculo.Restaurar(Guid.NewGuid(), "BRA2E19", "Toyota Corolla", 2023, StatusVeiculo.Disponivel),
            Veiculo.Restaurar(Guid.NewGuid(), "QWE4567", "Volkswagen Gol", 2019, StatusVeiculo.EmManutencao)
        };

        await veiculos.AlterarAsync(lista => lista.AddRange(iniciais.Select(VeiculoRegistro.De)));
    }
}
