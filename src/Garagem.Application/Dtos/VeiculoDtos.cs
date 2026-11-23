using Garagem.Domain.Entidades;

namespace Garagem.Application.Dtos;

/// <summary>Dados de entrada para cadastrar ou alterar um veículo.</summary>
public sealed record SalvarVeiculoDto(string Placa, string Modelo, int Ano, StatusVeiculo Status = StatusVeiculo.Disponivel);

/// <summary>Dados de saída expostos pela API (a entidade de domínio não sai do servidor).</summary>
public sealed record VeiculoDto(Guid Id, string Placa, string Modelo, int Ano, StatusVeiculo Status)
{
    public static VeiculoDto De(Veiculo v) => new(v.Id, v.Placa, v.Modelo, v.Ano, v.Status);
}
