using PubSubDemo.Core.Payroll;

namespace PubSubDemo.App;

/// <summary>
/// Generates a small, deterministic (seeded) stream of payroll entries so
/// the demo is reproducible. Stands in for "read from a real HR/timesheet
/// feed" - nothing else in the solution cares where RawPayrollEntries come
/// from.
/// </summary>
internal static class SamplePayInputSource
{
    private static readonly string[] EmployeeIds = { "EMP001", "EMP002", "EMP003" };

    public static IEnumerable<RawPayrollEntry> Generate(int employeeCount, int payPeriods, int seed)
    {
        var random = new Random(seed);
        var employeeIds = EmployeeIds.Take(employeeCount).ToArray();
        var salaries = employeeIds.ToDictionary(id => id, _ => 4_000m + (decimal)random.NextDouble() * 4_000m);
        var firstPeriod = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        for (var period = 0; period < payPeriods; period++)
        {
            var payPeriodUtc = firstPeriod.AddMonths(period);

            foreach (var employeeId in employeeIds)
            {
                var drift = (decimal)(random.NextDouble() - 0.5) * 1_000m;
                salaries[employeeId] = Math.Max(1_000m, salaries[employeeId] + drift);

                // A one-off bonus for EMP001, deliberately (not randomly) placed on the
                // third period so a sample run reliably demonstrates
                // PayrollAnomalyAlertSubscriber firing - both when the bonus lands and
                // again the following period when pay reverts to normal.
                var bonus = employeeId == "EMP001" && period == 2 ? 2_000m : 0m;

                yield return new RawPayrollEntry(employeeId, Math.Round(salaries[employeeId] + bonus, 2), payPeriodUtc);
            }
        }
    }
}
