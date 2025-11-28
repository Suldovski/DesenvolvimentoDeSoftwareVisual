namespace Luan.Services;

public static class CalculadoraAguaService
{
    public static (double consumoFaturado, double tarifa, double valorAgua, 
                   double adicionalBandeira, double taxaEsgoto, double total) 
        CalcularConta(double m3Consumidos, string bandeira, bool possuiEsgoto)
    {
        double consumoFaturado = m3Consumidos < 10 ? 10 : m3Consumidos;

        // Calcular valor da água por faixas (progressivo):
        // 0-10 -> 2.50, 11-20 -> 3.50, 21-50 -> 5.00, >50 -> 6.50
        double restante = consumoFaturado;
        double valorAgua = 0.0;

        // Faixa 0-10 (até 10 m3)
        double faixa = Math.Min(restante, 10);
        valorAgua += faixa * 2.50;
        restante -= faixa;

        if (restante > 0)
        {
            // Faixa 11-20 (próximos 10 m3)
            faixa = Math.Min(restante, 10);
            valorAgua += faixa * 3.50;
            restante -= faixa;
        }

        if (restante > 0)
        {
            // Faixa 21-50 (próximos 30 m3)
            faixa = Math.Min(restante, 30);
            valorAgua += faixa * 5.00;
            restante -= faixa;
        }

        if (restante > 0)
        {
            // Acima de 50
            valorAgua += restante * 6.50;
            restante = 0;
        }

        // Tarifa: calcular tarifa média por m3 faturado (útil para armazenar/mostrar)
        double tarifaMedia = consumoFaturado > 0 ? valorAgua / consumoFaturado : 0.0;

        double percentualBandeira = bandeira.ToLower() switch
        {
            "verde" => 0.00,
            "amarela" => 0.10,
            "vermelha" => 0.20,
            _ => 0.00
        };

        double adicionalBandeira = valorAgua * percentualBandeira;

        double taxaEsgoto = possuiEsgoto ? (valorAgua + adicionalBandeira) * 0.80 : 0;

        double total = valorAgua + adicionalBandeira + taxaEsgoto;

        return (consumoFaturado, tarifaMedia, valorAgua, adicionalBandeira, taxaEsgoto, total);
    }
}