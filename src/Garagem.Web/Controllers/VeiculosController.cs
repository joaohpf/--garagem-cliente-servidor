using Garagem.Web.Models;
using Garagem.Web.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Garagem.Web.Controllers;

/// <summary>Controller MVC do cliente: monta as telas a partir das respostas da API.</summary>
public sealed class VeiculosController : Controller
{
    private readonly IGaragemApiClient _api;

    public VeiculosController(IGaragemApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index() => View(await MontarPaginaAsync(new VeiculoForm()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastrar([Bind(Prefix = "Form")] VeiculoForm form)
    {
        if (!ModelState.IsValid)
            return View(nameof(Index), await MontarPaginaAsync(form));

        var resposta = await _api.CadastrarVeiculoAsync(form);
        if (!resposta.Sucesso)
        {
            ModelState.AddModelError(string.Empty, resposta.Erro!);
            return View(nameof(Index), await MontarPaginaAsync(form));
        }

        TempData["Sucesso"] = $"Veículo {resposta.Valor!.Placa} cadastrado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id)
    {
        var resposta = await _api.ObterVeiculoAsync(id);
        if (!resposta.Sucesso)
        {
            TempData["Erro"] = resposta.Erro;
            return RedirectToAction(nameof(Index));
        }

        var v = resposta.Valor!;
        ViewBag.Id = id;
        return View(new VeiculoForm { Placa = v.Placa, Modelo = v.Modelo, Ano = v.Ano, Status = v.Status });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, VeiculoForm form)
    {
        ViewBag.Id = id;
        if (!ModelState.IsValid)
            return View(form);

        var resposta = await _api.AtualizarVeiculoAsync(id, form);
        if (!resposta.Sucesso)
        {
            ModelState.AddModelError(string.Empty, resposta.Erro!);
            return View(form);
        }

        TempData["Sucesso"] = $"Veículo {resposta.Valor!.Placa} atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        var resposta = await _api.RemoverVeiculoAsync(id);
        TempData[resposta.Sucesso ? "Sucesso" : "Erro"] = resposta.Sucesso ? "Veículo removido." : resposta.Erro;
        return RedirectToAction(nameof(Index));
    }

    private async Task<VeiculosPagina> MontarPaginaAsync(VeiculoForm form)
    {
        var resposta = await _api.ListarVeiculosAsync();
        if (!resposta.Sucesso)
            ViewData["ErroApi"] = resposta.Erro;

        return new VeiculosPagina
        {
            Veiculos = resposta.Valor ?? Array.Empty<VeiculoModel>(),
            Form = form
        };
    }
}
