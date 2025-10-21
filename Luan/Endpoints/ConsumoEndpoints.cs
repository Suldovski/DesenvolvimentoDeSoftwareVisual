using Microsoft.EntityFrameworkCore;
using Luan.Data;
using Luan.Models;
using Luan.Services;
using Luan.DTOs;

namespace Luan.Endpoints;

public static class ConsumoEndpoints
{
    public static async Task<IResult> CadastrarConsumo(
        ConsumoAguaRequest request, 
        AppDbContext context)
    {
        // Validações básicas
        if (request.Mes is < 1 or > 12)
            return Results.BadRequest("Mês inválido. Deve ser entre 1 e 12.");
        
        if (request.Ano < 2000)
            return Results.BadRequest("Ano inválido. Deve ser maior ou igual a 2000.");
        
        if (request.M3Consumidos <= 0)
            return Results.BadRequest("Consumo deve ser maior que zero.");

        // Validar bandeira
        var bandeirasValidas = new[] { "verde", "amarela", "vermelha" };
        if (!bandeirasValidas.Contains(request.Bandeira.ToLower()))
            return Results.BadRequest("Bandeira inválida. Use: Verde, Amarela ou Vermelha.");

        // Verificar se já existe consumo para o mesmo CPF, mês e ano
        bool existe = await context.Consumos.AnyAsync(c => 
            c.Cpf == request.Cpf && c.Mes == request.Mes && c.Ano == request.Ano);
        
        if (existe)
            return Results.BadRequest("Consumo já cadastrado para esse CPF, mês e ano.");

        // Calcular valores
        var (faturado, tarifa, valor, adicional, esgoto, total) = 
            CalculadoraAguaService.CalcularConta(request.M3Consumidos, request.Bandeira, request.PossuiEsgoto);

        // Criar entidade
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

        // Salvar no banco
        context.Consumos.Add(consumo);
        await context.SaveChangesAsync();

        // Criar resposta
        var response = new ConsumoAguaResponse(
            consumo.Id, 
            consumo.Cpf!, 
            consumo.Mes, 
            consumo.Ano, 
            consumo.M3Consumidos,
            consumo.ConsumoFaturado, 
            consumo.Tarifa, 
            consumo.ValorAgua,
            consumo.AdicionalBandeira, 
            consumo.TaxaEsgoto, 
            consumo.Total
        );

        return Results.Created($"/api/consumo/buscar/{consumo.Cpf}/{consumo.Mes}/{consumo.Ano}", response);
    }

    public static async Task<IResult> ListarConsumos(AppDbContext context)
    {
        var lista = await context.Consumos
            .OrderBy(c => c.Ano)
            .ThenBy(c => c.Mes)
            .ThenBy(c => c.Cpf)
            .ToListAsync();
            
        if (!lista.Any())
            return Results.NotFound("Nenhum consumo encontrado.");

        var response = lista.Select(c => new ConsumoAguaResponse(
            c.Id, 
            c.Cpf!, 
            c.Mes, 
            c.Ano, 
            c.M3Consumidos,
            c.ConsumoFaturado, 
            c.Tarifa, 
            c.ValorAgua,
            c.AdicionalBandeira, 
            c.TaxaEsgoto, 
            c.Total
        ));

        return Results.Ok(response);
    }

    public static async Task<IResult> BuscarConsumo(
        string cpf, 
        int mes, 
        int ano, 
        AppDbContext context)
    {
        var consumo = await context.Consumos
            .FirstOrDefaultAsync(x => x.Cpf == cpf && x.Mes == mes && x.Ano == ano);
        
        if (consumo is null)
            return Results.NotFound("Consumo não encontrado.");

        var response = new ConsumoAguaResponse(
            consumo.Id, 
            consumo.Cpf!, 
            consumo.Mes, 
            consumo.Ano, 
            consumo.M3Consumidos,
            consumo.ConsumoFaturado, 
            consumo.Tarifa, 
            consumo.ValorAgua,
            consumo.AdicionalBandeira, 
            consumo.TaxaEsgoto, 
            consumo.Total
        );

        return Results.Ok(response);
    }

    public static async Task<IResult> RemoverConsumo(
        string cpf, 
        int mes, 
        int ano, 
        AppDbContext context)
    {
        var consumo = await context.Consumos
            .FirstOrDefaultAsync(x => x.Cpf == cpf && x.Mes == mes && x.Ano == ano);
        
        if (consumo is null)
            return Results.NotFound("Consumo não encontrado.");

        context.Consumos.Remove(consumo);
        await context.SaveChangesAsync();

        return Results.Ok(new { message = "Consumo removido com sucesso." });
    }

    public static async Task<IResult> TotalGeral(AppDbContext context)
    {
        var totalGeral = await context.Consumos.SumAsync(c => c.Total);
        
        if (totalGeral == 0)
            return Results.NotFound("Nenhum consumo encontrado para calcular o total geral.");

        var response = new TotalGeralResponse(totalGeral);
        return Results.Ok(response);
    }
}
