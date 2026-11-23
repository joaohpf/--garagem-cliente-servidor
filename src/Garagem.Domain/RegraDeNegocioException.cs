namespace Garagem.Domain;

/// <summary>
/// Indica que uma regra do domínio foi violada (ex.: placa inválida, período inválido).
/// A camada de aplicação converte esta exceção em um resultado de validação.
/// </summary>
public sealed class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string mensagem) : base(mensagem) { }
}
