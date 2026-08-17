namespace PubSubDemo.Core.Payroll;

/// <summary>
/// The "output" shape - what a <see cref="RawPayrollEntry"/> becomes after
/// the transform step has calculated statutory deductions. This is what
/// actually travels through the bus and what every subscriber consumes.
/// </summary>
public sealed record PayslipResult(
    string EmployeeId,
    decimal GrossPay,
    decimal EpfContribution,
    decimal Pcb,
    decimal NetPay,
    decimal? PreviousNetPay,
    decimal ChangePercent,
    PayTrend Trend,
    DateTimeOffset PayPeriodUtc);
