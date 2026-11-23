namespace Garagem.Application.Comum;

/// <summary>Tipos de falha de negócio. O controller traduz cada um para um código HTTP.</summary>
public enum TipoErro
{
    Nenhum = 0,
    Validacao,
    NaoEncontrado,
    Conflito
}

/// <summary>
/// Resultado de um caso de uso. Evita usar exceções para fluxos de negócio esperados
/// e mantém a camada de aplicação independente de HTTP.
/// </summary>
public class Resultado
{
    public bool Sucesso { get; }
    public TipoErro Erro { get; }
    public string? Mensagem { get; }

    protected Resultado(bool sucesso, TipoErro erro, string? mensagem)
    {
        Sucesso = sucesso;
        Erro = erro;
        Mensagem = mensagem;
    }

    public static Resultado Ok() => new(true, TipoErro.Nenhum, null);
    public static Resultado Falha(TipoErro erro, string mensagem) => new(false, erro, mensagem);
}

public sealed class Resultado<T> : Resultado
{
    public T? Valor { get; }

    private Resultado(bool sucesso, T? valor, TipoErro erro, string? mensagem) : base(sucesso, erro, mensagem)
    {
        Valor = valor;
    }

    public static Resultado<T> Ok(T valor) => new(true, valor, TipoErro.Nenhum, null);
    public static new Resultado<T> Falha(TipoErro erro, string mensagem) => new(false, default, erro, mensagem);
}
