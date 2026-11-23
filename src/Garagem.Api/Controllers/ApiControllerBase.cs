using Garagem.Application.Comum;
using Microsoft.AspNetCore.Mvc;

namespace Garagem.Api.Controllers;

/// <summary>
/// Traduz o resultado de negócio para uma resposta HTTP pública,
/// sem expor detalhes internos (seção 3.2 da parte teórica).
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Falha(Resultado resultado)
    {
        var (status, titulo) = resultado.Erro switch
        {
            TipoErro.Validacao => (StatusCodes.Status400BadRequest, "Dados inválidos"),
            TipoErro.NaoEncontrado => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            TipoErro.Conflito => (StatusCodes.Status409Conflict, "Conflito com o estado atual"),
            _ => (StatusCodes.Status500InternalServerError, "Erro inesperado")
        };

        return Problem(detail: resultado.Mensagem, statusCode: status, title: titulo);
    }
}
