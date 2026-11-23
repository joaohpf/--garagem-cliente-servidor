using Garagem.Web.Models;
using Garagem.Web.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Garagem.Web.Controllers;

public sealed class ReservasController : Controller
{
    private readonly IGaragemApiClient _api;

    public ReservasController(IGaragemApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index() => View(await MontarPaginaAsync(new ReservaForm()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar([Bind(Prefix = "Form")] ReservaForm form)
    {
        if (!ModelState.IsValid)
            return View(nameof(Index), await MontarPaginaAsync(form));

        var resposta = await _api.CriarReservaAsync(form);
        if (!resposta.Sucesso)
        {
            // Ex.: conflito de datas devolvido pelo servidor (HTTP 409).
            ModelState.AddModelError(string.Empty, resposta.Erro!);
            return View(nameof(Index), await MontarPaginaAsync(form));
        }

        var r = resposta.Valor!;
        TempData["Sucesso"] = $"Reserva do veículo {r.PlacaVeiculo} criada para {r.Cliente}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var resposta = await _api.CancelarReservaAsync(id);
        TempData[resposta.Sucesso ? "Sucesso" : "Erro"] = resposta.Sucesso ? "Reserva cancelada." : resposta.Erro;
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Erro() => View();

    private async Task<ReservasPagina> MontarPaginaAsync(ReservaForm form)
    {
        var reservas = await _api.ListarReservasAsync();
        var veiculos = await _api.ListarVeiculosAsync();

        var erro = reservas.Erro ?? veiculos.Erro;
        if (erro is not null)
            ViewData["ErroApi"] = erro;

        return new ReservasPagina
        {
            Reservas = reservas.Valor ?? Array.Empty<ReservaModel>(),
            Veiculos = veiculos.Valor ?? Array.Empty<VeiculoModel>(),
            Form = form
        };
    }
}
