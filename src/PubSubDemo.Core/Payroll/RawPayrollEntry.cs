namespace PubSubDemo.Core.Payroll;

/// <summary>
/// The "input" shape - a single employee's raw basic salary for one pay
/// period, exactly as it would arrive from a timesheet or HR feed.
/// Intentionally minimal; this is the wire format, not what subscribers
/// want to look at.
/// </summary>
public sealed record RawPayrollEntry(string EmployeeId, decimal BasicSalary, DateTimeOffset PayPeriodUtc);
