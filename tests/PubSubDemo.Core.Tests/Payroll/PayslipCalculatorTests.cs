using PubSubDemo.Core.Payroll;
using Xunit;

namespace PubSubDemo.Core.Tests.Payroll;

public class PayslipCalculatorTests
{
    [Fact]
    public void Transform_ComputesEpfAndNetPayFromGrossPay()
    {
        var calculator = new PayslipCalculator(new StubTaxCalculator(pcb: 80m));
        var entry = new RawPayrollEntry("EMP001", 6000m, DateTimeOffset.UtcNow);

        var result = calculator.Transform(entry);

        Assert.Equal(6000m, result.GrossPay);
        Assert.Equal(660.00m, result.EpfContribution); // 11% of 6000
        Assert.Equal(80m, result.Pcb);
        Assert.Equal(5260.00m, result.NetPay); // 6000 - 660 - 80
    }

    [Fact]
    public void Transform_FirstEntryForEmployee_HasNoPreviousNetPayAndZeroChange()
    {
        var calculator = new PayslipCalculator(new StubTaxCalculator(pcb: 0m));
        var entry = new RawPayrollEntry("EMP001", 5000m, DateTimeOffset.UtcNow);

        var result = calculator.Transform(entry);

        Assert.Null(result.PreviousNetPay);
        Assert.Equal(0m, result.ChangePercent);
        Assert.Equal(PayTrend.Unchanged, result.Trend);
    }

    [Fact]
    public void Transform_NetPayIncrease_ReturnsPositiveChangeAndIncreasedTrend()
    {
        var calculator = new PayslipCalculator(new StubTaxCalculator(pcb: 0m));
        calculator.Transform(new RawPayrollEntry("EMP001", 5000m, DateTimeOffset.UtcNow));

        var result = calculator.Transform(new RawPayrollEntry("EMP001", 6000m, DateTimeOffset.UtcNow));

        Assert.Equal(4450m, result.PreviousNetPay); // 5000 - 550
        Assert.True(result.ChangePercent > 0m);
        Assert.Equal(PayTrend.Increased, result.Trend);
    }

    [Fact]
    public void Transform_NetPayDecrease_ReturnsNegativeChangeAndDecreasedTrend()
    {
        var calculator = new PayslipCalculator(new StubTaxCalculator(pcb: 0m));
        calculator.Transform(new RawPayrollEntry("EMP001", 6000m, DateTimeOffset.UtcNow));

        var result = calculator.Transform(new RawPayrollEntry("EMP001", 5000m, DateTimeOffset.UtcNow));

        Assert.True(result.ChangePercent < 0m);
        Assert.Equal(PayTrend.Decreased, result.Trend);
    }

    [Fact]
    public void Transform_TracksEachEmployeeIndependently()
    {
        var calculator = new PayslipCalculator(new StubTaxCalculator(pcb: 0m));
        calculator.Transform(new RawPayrollEntry("EMP001", 5000m, DateTimeOffset.UtcNow));
        calculator.Transform(new RawPayrollEntry("EMP002", 3000m, DateTimeOffset.UtcNow));

        var result = calculator.Transform(new RawPayrollEntry("EMP002", 3300m, DateTimeOffset.UtcNow));

        Assert.Equal(2670m, result.PreviousNetPay); // 3000 - 330
        Assert.Equal(PayTrend.Increased, result.Trend);
    }

    [Fact]
    public void Constructor_WithNullTaxCalculator_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PayslipCalculator(null!));
    }

    private sealed class StubTaxCalculator : ITaxCalculator
    {
        private readonly decimal _pcb;

        public StubTaxCalculator(decimal pcb) => _pcb = pcb;

        public decimal Calculate(decimal grossPay) => _pcb;
    }
}
