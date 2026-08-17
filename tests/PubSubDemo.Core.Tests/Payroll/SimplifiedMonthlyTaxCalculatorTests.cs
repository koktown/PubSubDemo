using PubSubDemo.Core.Payroll;
using Xunit;

namespace PubSubDemo.Core.Tests.Payroll;

public class SimplifiedMonthlyTaxCalculatorTests
{
    private readonly SimplifiedMonthlyTaxCalculator _calculator = new();

    [Fact]
    public void Calculate_WithinTaxFreeBand_ReturnsZero()
    {
        Assert.Equal(0m, _calculator.Calculate(4500m));
    }

    [Fact]
    public void Calculate_AtTaxFreeThreshold_ReturnsZero()
    {
        Assert.Equal(0m, _calculator.Calculate(5000m));
    }

    [Fact]
    public void Calculate_InSecondBand_TaxesOnlyTheAmountAboveTheThreshold()
    {
        // 5,000 tax-free + 1,000 taxed at 8% = 80
        Assert.Equal(80m, _calculator.Calculate(6000m));
    }

    [Fact]
    public void Calculate_InTopBand_TaxesEachBandAtItsOwnRate()
    {
        // 5,000 @ 0% + 5,000 @ 8% (400) + 2,000 @ 15% (300) = 700
        Assert.Equal(700m, _calculator.Calculate(12000m));
    }

    [Fact]
    public void Calculate_WithZeroGrossPay_ReturnsZero()
    {
        Assert.Equal(0m, _calculator.Calculate(0m));
    }

    [Fact]
    public void Calculate_WithNegativeGrossPay_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.Calculate(-1m));
    }
}
