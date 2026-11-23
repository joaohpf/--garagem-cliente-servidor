using Garagem.Web.Servicos;

// ============================================================
// CLIENTE web (ASP.NET Core MVC). Não acessa dados diretamente:
// toda operação vira uma requisição HTTP para a Garagem.Api.
// ============================================================

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Cliente HTTP tipado, injetado nos controllers por meio da interface.
var urlApi = builder.Configuration["Api:UrlBase"] ?? "http://localhost:5080/";
builder.Services.AddHttpClient<IGaragemApiClient, GaragemApiClient>(http =>
{
    http.BaseAddress = new Uri(urlApi);
    http.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Reservas/Erro");

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reservas}/{action=Index}/{id?}");

app.Run();
