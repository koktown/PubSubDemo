namespace PubSubDemo.Core.Payroll;

/// <summary>
/// Turns a gross pay figure into a monthly tax deduction (PCB). Kept as its
/// own seam, separate from <see cref="PayslipCalculator"/>, because tax
/// rules are the part of a payroll system most likely to change - by
/// jurisdiction, by tax year, or by employee category - without anything
/// else in the pipeline needing to know.
/// </summary>
public interface ITaxCalculator
{
    decimal Calculate(decimal grossPay);
}
