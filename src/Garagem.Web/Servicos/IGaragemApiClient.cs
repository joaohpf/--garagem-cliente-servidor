using Garagem.Web.Models;

namespace Garagem.Web.Servicos;

/// <summary>
/// Abstração do servidor do ponto de vista do cliente.
/// Os controllers dependem desta interface, não do HttpClient.
/// </summary>
public interface IGaragemApiClient
{
    Task<RespostaApi<IReadOnlyList<VeiculoModel>>> ListarVeiculosAsync();
    Task<RespostaApi<VeiculoModel>> ObterVeiculoAsync(Guid id);
    Task<RespostaApi<VeiculoModel>> CadastrarVeiculoAsync(VeiculoForm form);
    Task<RespostaApi<VeiculoModel>> AtualizarVeiculoAsync(Guid id, VeiculoForm form);
    Task<RespostaApi<bool>> RemoverVeiculoAsync(Guid id);

    Task<RespostaApi<IReadOnlyList<ReservaModel>>> ListarReservasAsync();
    Task<RespostaApi<ReservaModel>> CriarReservaAsync(ReservaForm form);
    Task<RespostaApi<ReservaModel>> CancelarReservaAsync(Guid id);
}
