namespace Luan.DTOs;

public record ConsumoAguaRequest(
    string Cpf,
    int Mes,
    int Ano,
    double M3Consumidos,
    string Bandeira,
    bool PossuiEsgoto
);
