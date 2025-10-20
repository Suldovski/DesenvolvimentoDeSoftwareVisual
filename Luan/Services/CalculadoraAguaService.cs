namespace Luan.Services;

public static class CalculadoraAguaService
{
    public static (double consumoFaturado, double tarifa, double valorAgua, 
                   double adicionalBandeira, double taxaEsgoto, double total) 
        CalcularConta(double m3Consumidos, string bandeira, bool possuiEsgoto)
    {
        
        double consumoFaturado = m3Consumidos < 10 ? 10 : m3Consumidos;

        double tarifa = consumoFaturado switch
        {
            <= 10 => 2.50,
            <= 20 => 3.50,
            <= 50 => 5.00,
            _ => 6.50
        };

       
        double valorAgua = consumoFaturado * tarifa;


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

        return (consumoFaturado, tarifa, valorAgua, adicionalBandeira, taxaEsgoto, total);
    }
}