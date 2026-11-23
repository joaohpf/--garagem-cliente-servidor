using Garagem.Application.Dtos;
using Garagem.Application.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Garagem.Api.Controllers;

/// <summary>
/// Controller da API: recebe a requisição, chama o serviço e devolve a resposta.
/// Não contém regra de negócio nem acesso a dados.
/// </summary>
[Route("api/veiculos")]
public sealed class VeiculosController : ApiControllerBase
{
    private readonly IVeiculoService _servico;

    public VeiculosController(IVeiculoService servico)
    {
        _servico = servico;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VeiculoDto>>> Listar() =>
        Ok(await _servico.ListarAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var resultado = await _servico.ObterAsync(id);
        return resultado.Sucesso ? Ok(resultado.Valor) : Falha(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar(SalvarVeiculoDto dados)
    {
        var resultado = await _servico.CadastrarAsync(dados);
        return resultado.Sucesso
            ? CreatedAtAction(nameof(Obter), new { id = resultado.Valor!.Id }, resultado.Valor)
            : Falha(resultado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, SalvarVeiculoDto dados)
    {
        var resultado = await _servico.AtualizarAsync(id, dados);
        return resultado.Sucesso ? Ok(resultado.Valor) : Falha(resultado);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var resultado = await _servico.RemoverAsync(id);
        return resultado.Sucesso ? NoContent() : Falha(resultado);
    }
}
