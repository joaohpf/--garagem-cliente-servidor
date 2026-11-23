using System.Text.Json.Serialization;
using Garagem.Application.Servicos;
using Garagem.Domain.Repositorios;
using Garagem.Infrastructure.Persistencia;
using Garagem.Infrastructure.Repositorios;

// ============================================================
// SERVIDOR (camada de apresentação da API + raiz de composição)
// Aqui as abstrações são ligadas às implementações concretas.
// Trocar JSON por banco relacional exigiria mudar só este bloco
// e a camada de infraestrutura.
// ============================================================

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();

// --- Camada de dados (infraestrutura) ---
var pastaDados = Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["Persistencia:Pasta"] ?? "data");
builder.Services.AddSingleton(new ArquivoJson<VeiculoRegistro>(Path.Combine(pastaDados, "veiculos.json")));
builder.Services.AddSingleton(new ArquivoJson<ReservaRegistro>(Path.Combine(pastaDados, "reservas.json")));
builder.Services.AddScoped<IVeiculoRepository, JsonVeiculoRepository>();
builder.Services.AddScoped<IReservaRepository, JsonReservaRepository>();

// --- Camada de aplicação (serviços) ---
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
builder.Services.AddScoped<IReservaService, ReservaService>();

var app = builder.Build();

await DadosIniciais.CarregarAsync(app.Services.GetRequiredService<ArquivoJson<VeiculoRegistro>>());

app.UseExceptionHandler();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/api/status"));

app.Run();
