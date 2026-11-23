using Garagem.Application.Dtos;
using Garagem.Application.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Garagem.Api.Controllers;

[Route("api/reservas")]
public sealed class ReservasController : ApiControllerBase
{
    private readonly IReservaService _servico;

    public ReservasController(IReservaService servico)
    {
        _servico = servico;
    }

    /// <summary>Lista reservas; opcionalmente filtra por veículo (?veiculoId=...).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReservaDto>>> Listar([FromQuery] Guid? veiculoId) =>
        Ok(await _servico.ListarAsync(veiculoId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var resultado = await _servico.ObterAsync(id);
        return resultado.Sucesso ? Ok(resultado.Valor) : Falha(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Reservar(CriarReservaDto dados)
    {
        var resultado = await _servico.ReservarAsync(dados);
        return resultado.Sucesso
            ? CreatedAtAction(nameof(Obter), new { id = resultado.Valor!.Id }, resultado.Valor)
            : Falha(resultado);
    }

    [HttpPatch("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var resultado = await _servico.CancelarAsync(id);
        return resultado.Sucesso ? Ok(resultado.Valor) : Falha(resultado);
    }
}
