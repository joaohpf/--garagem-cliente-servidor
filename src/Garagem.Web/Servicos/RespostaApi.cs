namespace Garagem.Web.Servicos;

/// <summary>Resposta da API já interpretada pelo cliente.</summary>
public sealed class RespostaApi<T>
{
    public bool Sucesso { get; private init; }
    public T? Valor { get; private init; }
    public string? Erro { get; private init; }

    public static RespostaApi<T> Ok(T valor) => new() { Sucesso = true, Valor = valor };
    public static RespostaApi<T> Falha(string erro) => new() { Sucesso = false, Erro = erro };
}
