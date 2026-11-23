using System.Net.Http.Json;
using System.Text.Json;
using Garagem.Web.Models;

namespace Garagem.Web.Servicos;

/// <summary>
/// Implementação HTTP de IGaragemApiClient. Converte respostas de erro (ProblemDetails)
/// e falhas de comunicação em mensagens que a interface consegue exibir.
/// </summary>
public sealed class GaragemApiClient : IGaragemApiClient
{
    private const string MensagemServidorIndisponivel =
        "Não foi possível conectar ao servidor (Garagem.Api). Verifique se a API está em execução.";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly ILogger<GaragemApiClient> _log;

    public GaragemApiClient(HttpClient http, ILogger<GaragemApiClient> log)
    {
        _http = http;
        _log = log;
    }

    // ---------------- Veículos ----------------

    public Task<RespostaApi<IReadOnlyList<VeiculoModel>>> ListarVeiculosAsync() =>
        EnviarAsync<IReadOnlyList<VeiculoModel>>(() => _http.GetAsync("api/veiculos"));

    public Task<RespostaApi<VeiculoModel>> ObterVeiculoAsync(Guid id) =>
        EnviarAsync<VeiculoModel>(() => _http.GetAsync($"api/veiculos/{id}"));

    public Task<RespostaApi<VeiculoModel>> CadastrarVeiculoAsync(VeiculoForm form) =>
        EnviarAsync<VeiculoModel>(() => _http.PostAsJsonAsync("api/veiculos", CorpoVeiculo(form), Json));

    public Task<RespostaApi<VeiculoModel>> AtualizarVeiculoAsync(Guid id, VeiculoForm form) =>
        EnviarAsync<VeiculoModel>(() => _http.PutAsJsonAsync($"api/veiculos/{id}", CorpoVeiculo(form), Json));

    public Task<RespostaApi<bool>> RemoverVeiculoAsync(Guid id) =>
        EnviarAsync<bool>(() => _http.DeleteAsync($"api/veiculos/{id}"), semCorpo: true);

    // ---------------- Reservas ----------------

    public Task<RespostaApi<IReadOnlyList<ReservaModel>>> ListarReservasAsync() =>
        EnviarAsync<IReadOnlyList<ReservaModel>>(() => _http.GetAsync("api/reservas"));

    public Task<RespostaApi<ReservaModel>> CriarReservaAsync(ReservaForm form) =>
        EnviarAsync<ReservaModel>(() => _http.PostAsJsonAsync("api/reservas", new
        {
            veiculoId = form.VeiculoId,
            cliente = form.Cliente,
            inicio = DateOnly.FromDateTime(form.Inicio),
            fim = DateOnly.FromDateTime(form.Fim)
        }, Json));

    public Task<RespostaApi<ReservaModel>> CancelarReservaAsync(Guid id) =>
        EnviarAsync<ReservaModel>(() => _http.PatchAsync($"api/reservas/{id}/cancelar", null));

    // ---------------- Infraestrutura de comunicação ----------------

    private static object CorpoVeiculo(VeiculoForm form) =>
        new { placa = form.Placa, modelo = form.Modelo, ano = form.Ano, status = form.Status };

    private async Task<RespostaApi<T>> EnviarAsync<T>(Func<Task<HttpResponseMessage>> requisicao, bool semCorpo = false)
    {
        try
        {
            using var resposta = await requisicao();

            if (resposta.IsSuccessStatusCode)
            {
                if (semCorpo)
                    return RespostaApi<T>.Ok(default!);

                var valor = await resposta.Content.ReadFromJsonAsync<T>(Json);
                return RespostaApi<T>.Ok(valor!);
            }

            return RespostaApi<T>.Falha(await LerMensagemDeErroAsync(resposta));
        }
        catch (HttpRequestException ex)
        {
            _log.LogWarning(ex, "Falha de comunicação com a API.");
            return RespostaApi<T>.Falha(MensagemServidorIndisponivel);
        }
        catch (TaskCanceledException ex)
        {
            _log.LogWarning(ex, "Tempo de resposta da API esgotado.");
            return RespostaApi<T>.Falha("O servidor demorou para responder. Tente novamente.");
        }
    }

    private static async Task<string> LerMensagemDeErroAsync(HttpResponseMessage resposta)
    {
        try
        {
            var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>(Json);
            if (problema.ValueKind == JsonValueKind.Object)
            {
                if (problema.TryGetProperty("detail", out var detalhe) && detalhe.ValueKind == JsonValueKind.String)
                    return detalhe.GetString()!;

                // Erros de formato gerados pelo [ApiController] trazem a lista em "errors".
                if (problema.TryGetProperty("errors", out var erros) && erros.ValueKind == JsonValueKind.Object)
                {
                    var mensagens = erros.EnumerateObject()
                        .SelectMany(p => p.Value.EnumerateArray().Select(m => m.GetString()))
                        .Where(m => !string.IsNullOrWhiteSpace(m));
                    return string.Join(" ", mensagens);
                }

                if (problema.TryGetProperty("title", out var titulo) && titulo.ValueKind == JsonValueKind.String)
                    return titulo.GetString()!;
            }
        }
        catch (JsonException)
        {
            // Corpo sem JSON: usa a mensagem genérica abaixo.
        }

        return $"O servidor recusou a operação (HTTP {(int)resposta.StatusCode}).";
    }
}
