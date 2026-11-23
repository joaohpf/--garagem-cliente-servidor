using Microsoft.AspNetCore.Mvc;

namespace Garagem.Api.Controllers;

/// <summary>Permite ao cliente verificar se o servidor está no ar.</summary>
[ApiController]
[Route("api/status")]
public sealed class StatusController : ControllerBase
{
    private readonly TimeProvider _relogio;

    public StatusController(TimeProvider relogio)
    {
        _relogio = relogio;
    }

    [HttpGet]
    public IActionResult Obter() => Ok(new
    {
        servico = "Garagem.Api",
        situacao = "online",
        horario = _relogio.GetLocalNow()
    });
}
