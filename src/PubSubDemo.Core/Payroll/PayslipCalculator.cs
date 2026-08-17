using System.Collections.Concurrent;
using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.Payroll;

/// <summary>
/// Turns a raw payroll entry into a <see cref="PayslipResult"/> by applying
/// statutory deductions (EPF, PCB) and comparing the resulting net pay
/// against the last pay period seen for that employee. It is stateful (one
/// "last net pay" per employee) but the state is fully encapsulated here -
/// no bus, no console, no subscribers - which is what makes it trivial to
/// unit test on its own.
/// </summary>
public sealed class PayslipCalculator : IDataTransformer<RawPayrollEntry, PayslipResult>
{
    private const decimal EmployeeEpfRate = 0.11m;
    private const decimal UnchangedThresholdPercent = 0.01m;

    private readonly ITaxCalculator _taxCalculator;
    private readonly ConcurrentDictionary<string, decimal> _lastNetPayByEmployee = new();

    public PayslipCalculator(ITaxCalculator taxCalculator)
    {
        _taxCalculator = taxCalculator ?? throw new ArgumentNullException(nameof(taxCalculator));
    }

    public PayslipResult Transform(RawPayrollEntry input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var grossPay = input.BasicSalary;
        var epf = Math.Round(grossPay * EmployeeEpfRate, 2);
        var pcb = _taxCalculator.Calculate(grossPay);
        var netPay = grossPay - epf - pcb;

        decimal? previousNetPay = _lastNetPayByEmployee.TryGetValue(input.EmployeeId, out var last)
            ? last
            : null;

        var changePercent = previousNetPay is > 0m
            ? Math.Round((netPay - previousNetPay.Value) / previousNetPay.Value * 100m, 2)
            : 0m;

        var trend = changePercent switch
        {
            > UnchangedThresholdPercent => PayTrend.Increased,
            < -UnchangedThresholdPercent => PayTrend.Decreased,
            _ => PayTrend.Unchanged
        };

        _lastNetPayByEmployee[input.EmployeeId] = netPay;

        return new PayslipResult(input.EmployeeId, grossPay, epf, pcb, netPay, previousNetPay, changePercent, trend, input.PayPeriodUtc);
    }
}
