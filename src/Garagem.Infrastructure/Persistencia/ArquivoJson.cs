using System.Text.Json;
using System.Text.Json.Serialization;

namespace Garagem.Infrastructure.Persistencia;

/// <summary>
/// Armazenamento simples em arquivo JSON, usado como camada de dados do protótipo.
/// Registrado como singleton: uma única instância controla o acesso ao arquivo.
/// </summary>
public sealed class ArquivoJson<T>
{
    private static readonly JsonSerializerOptions Opcoes = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _caminho;
    private readonly SemaphoreSlim _trava = new(1, 1);

    public ArquivoJson(string caminho)
    {
        _caminho = caminho;
        var pasta = Path.GetDirectoryName(Path.GetFullPath(caminho));
        if (!string.IsNullOrEmpty(pasta))
            Directory.CreateDirectory(pasta);
    }

    public bool Existe => File.Exists(_caminho);

    public async Task<List<T>> LerAsync()
    {
        await _trava.WaitAsync();
        try
        {
            return await LerSemTravaAsync();
        }
        finally
        {
            _trava.Release();
        }
    }

    /// <summary>Lê, altera e grava a lista como uma única operação protegida.</summary>
    public async Task AlterarAsync(Action<List<T>> alteracao)
    {
        await _trava.WaitAsync();
        try
        {
            var itens = await LerSemTravaAsync();
            alteracao(itens);

            // Grava em arquivo temporário e substitui, para não deixar o JSON pela metade.
            var temporario = _caminho + ".tmp";
            await File.WriteAllTextAsync(temporario, JsonSerializer.Serialize(itens, Opcoes));
            File.Move(temporario, _caminho, overwrite: true);
        }
        finally
        {
            _trava.Release();
        }
    }

    private async Task<List<T>> LerSemTravaAsync()
    {
        if (!File.Exists(_caminho))
            return new List<T>();

        var conteudo = await File.ReadAllTextAsync(_caminho);
        return string.IsNullOrWhiteSpace(conteudo)
            ? new List<T>()
            : JsonSerializer.Deserialize<List<T>>(conteudo, Opcoes) ?? new List<T>();
    }
}
