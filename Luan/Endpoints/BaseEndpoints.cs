using Luan.DTOs;

namespace Luan.Endpoints;

public static class BaseEndpoints
{
    public static void MapConsumoEndpoints(this WebApplication app)
    {
        // Grupo para organizar no Swagger
        var group = app.MapGroup("/api/consumo")
                       .WithTags("Consumo de Água");

        group.MapPost("/cadastrar", ConsumoEndpoints.CadastrarConsumo);
        group.MapGet("/listar", ConsumoEndpoints.ListarConsumos);
        group.MapGet("/buscar/{cpf}/{mes:int}/{ano:int}", ConsumoEndpoints.BuscarConsumo);
        group.MapDelete("/remover/{cpf}/{mes:int}/{ano:int}", ConsumoEndpoints.RemoverConsumo);
        group.MapGet("/total-geral", ConsumoEndpoints.TotalGeral);
    }
}