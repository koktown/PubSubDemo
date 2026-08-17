namespace PubSubDemo.Core.Payroll;

/// <summary>
/// Illustrative progressive tax brackets loosely modeled on how a monthly
/// tax deduction (PCB) scheme works: each band of gross pay is taxed at its
/// own rate, and only the portion of pay that falls inside a band is taxed
/// at that band's rate. The thresholds and rates here are simplified for
/// the demo and are not real tax figures - a production system would plug
/// a jurisdiction-compliant calculator in behind the same
/// <see cref="ITaxCalculator"/> interface.
/// </summary>
public sealed class SimplifiedMonthlyTaxCalculator : ITaxCalculator
{
    private static readonly (decimal UpperBound, decimal Rate)[] Bands =
    {
        (5_000m, 0.00m),
        (10_000m, 0.08m),
        (decimal.MaxValue, 0.15m)
    };

    public decimal Calculate(decimal grossPay)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(grossPay);

        var tax = 0m;
        var lowerBound = 0m;

        foreach (var (upperBound, rate) in Bands)
        {
            if (grossPay <= lowerBound)
            {
                break;
            }

            var taxableInBand = Math.Min(grossPay, upperBound) - lowerBound;
            tax += taxableInBand * rate;
            lowerBound = upperBound;
        }

        return Math.Round(tax, 2);
    }
}
