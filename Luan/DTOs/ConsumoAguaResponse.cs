namespace Luan.DTOs;

public record ConsumoAguaResponse(
    int Id,
    string Cpf,
    int Mes,
    int Ano,
    double M3Consumidos,
    double ConsumoFaturado,
    double Tarifa,
    double ValorAgua,
    double AdicionalBandeira,
    double TaxaEsgoto,
    double Total
);