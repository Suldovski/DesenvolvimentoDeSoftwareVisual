using Xunit;
using Luan.Services;

namespace Luan.Tests;

public class CalculadoraAguaServiceTests
{
    [Theory]
    [InlineData(5, "verde", false, 10, 25.0)] // consumo mínimo 10, valorAgua 25
    [InlineData(10, "verde", false, 10, 25.0)]
    [InlineData(15, "verde", false, 15, 42.5)] // 10*2.5 + 5*3.5 = 25 + 17.5 = 42.5
    [InlineData(35, "amarela", false, 35, 135.0)] // 10*2.5 + 10*3.5 + 15*5 = 25 + 35 + 75 = 135
    public void CalcularConta_ValorAguaETarifaMedia_Esperado(double m3, string bandeira, bool esgoto, double expectedFaturado, double expectedValorAgua)
    {
        var (faturado, tarifa, valorAgua, adicional, taxaEsgoto, total) = CalculadoraAguaService.CalcularConta(m3, bandeira, esgoto);

        Assert.Equal(expectedFaturado, faturado, 6);
        Assert.Equal(expectedValorAgua, valorAgua, 6);
        // tarifa média: permitir pequena variação
        Assert.Equal(valorAgua / faturado, tarifa, 6);
    }

    [Fact]
    public void CalcularConta_BandeiraEsgoto_CalculoCompleto()
    {
        // Exemplo: 20 m3, bandeira vermelha (20%), possui esgoto
        var (faturado, tarifa, valorAgua, adicional, taxaEsgoto, total) = CalculadoraAguaService.CalcularConta(20, "vermelha", true);

        // valorAgua: 10*2.5 + 10*3.5 = 25 + 35 = 60
        Assert.Equal(20, faturado, 6);
        Assert.Equal(60.0, valorAgua, 6);
        Assert.Equal(0.20 * 60.0, adicional, 6);
        Assert.Equal((60.0 + adicional) * 0.80, taxaEsgoto, 6);
        Assert.Equal(60.0 + adicional + taxaEsgoto, total, 6);
    }
}
