using Microsoft.EntityFrameworkCore;
using Luan.Data;
using Luan.Models;
using Luan.Services;
using Luan.DTOs;
using System.Diagnostics.Tracing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/consumo/cadastrar", async (ConsumoAguaRequest request, AppDbContext context) =>
{
    if (request.Mes is < 1 or > 12) 
        return Results.BadRequest("Mês inválido.");
    
    if (request.Ano < 2000) 
        return Results.BadRequest("Ano inválido.");
    
    if (request.M3Consumidos <= 0) 
        return Results.BadRequest("Consumo deve ser maior que zero.");

    
    var bandeirasValidas = new[] { "verde", "amarela", "vermelha" };
    if (!bandeirasValidas.Contains(request.Bandeira.ToLower()))
        return Results.BadRequest("Bandeira inválida. Use: Verde, Amarela ou Vermelha.");

    bool existe = await context.Consumos.AnyAsync(c => 
        c.Cpf == request.Cpf && c.Mes == request.Mes && c.Ano == request.Ano);
    
    if (existe) 
        return Results.BadRequest("Consumo já cadastrado para esse CPF, mês e ano.");

    var (faturado, tarifa, valor, adicional, esgoto, total) = 
        CalculadoraAguaService.CalcularConta(request.M3Consumidos, request.Bandeira, request.PossuiEsgoto);

    var consumo = new ConsumoAgua
    {
        Cpf = request.Cpf,
        Mes = request.Mes,
        Ano = request.Ano,
        M3Consumidos = request.M3Consumidos,
        Bandeira = request.Bandeira,
        PossuiEsgoto = request.PossuiEsgoto,
        ConsumoFaturado = faturado,
        Tarifa = tarifa,
        ValorAgua = valor,
        AdicionalBandeira = adicional,
        TaxaEsgoto = esgoto,
        Total = total
    };

    context.Consumos.Add(consumo);
    await context.SaveChangesAsync();

    var response = new ConsumoAguaResponse(
        consumo.Id, consumo.Cpf!, consumo.Mes, consumo.Ano, consumo.M3Consumidos,
        consumo.ConsumoFaturado, consumo.Tarifa, consumo.ValorAgua,
        consumo.AdicionalBandeira, consumo.TaxaEsgoto, consumo.Total
    );

    return Results.Created($"/api/consumo/buscar/{consumo.Cpf}/{consumo.Mes}/{consumo.Ano}", response);
});


app.MapGet("/api/consumo/listar", async (AppDbContext context) =>
{
    var lista = await context.Consumos.ToListAsync();
    if (!lista.Any()) 
        return Results.NotFound("Nenhum consumo encontrado.");

    var response = lista.Select(c => new ConsumoAguaResponse(
        c.Id, c.Cpf!, c.Mes, c.Ano, c.M3Consumidos,
        c.ConsumoFaturado, c.Tarifa, c.ValorAgua,
        c.AdicionalBandeira, c.TaxaEsgoto, c.Total
    ));

    return Results.Ok(response);
});


app.MapGet("/api/consumo/buscar/{cpf}/{mes:int}/{ano:int}", async (string cpf, int mes, int ano, AppDbContext context) =>
{
    var consumo = await context.Consumos.FirstOrDefaultAsync(x => 
        x.Cpf == cpf && x.Mes == mes && x.Ano == ano);
    
    if (consumo is null) 
        return Results.NotFound("Consumo não encontrado.");

    var response = new ConsumoAguaResponse(
        consumo.Id, consumo.Cpf!, consumo.Mes, consumo.Ano, consumo.M3Consumidos,
        consumo.ConsumoFaturado, consumo.Tarifa, consumo.ValorAgua,
        consumo.AdicionalBandeira, consumo.TaxaEsgoto, consumo.Total
    );

    return Results.Ok(response);
});


app.MapDelete("/api/consumo/remover/{cpf}/{mes:int}/{ano:int}", async (string cpf, int mes, int ano, AppDbContext context) =>
{
    var consumo = await context.Consumos.FirstOrDefaultAsync(x => 
        x.Cpf == cpf && x.Mes == mes && x.Ano == ano);
    
    if (consumo is null) 
        return Results.NotFound("Consumo não encontrado.");

    context.Consumos.Remove(consumo);
    await context.SaveChangesAsync();

    return Results.Ok("Consumo removido com sucesso.");
});


app.MapGet("/api/consumo/total-geral", async (AppDbContext context) =>
{
    var totalGeral = await context.Consumos.SumAsync(c => c.Total);
    
    if (totalGeral == 0) 
        return Results.NotFound("Nenhum consumo encontrado para calcular o total geral.");

    var response = new TotalGeralResponse(totalGeral);
    return Results.Ok(response);
});

app.Run();