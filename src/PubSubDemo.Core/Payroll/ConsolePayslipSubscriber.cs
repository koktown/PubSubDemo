using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.Payroll;

/// <summary>
/// Displays every payslip as a single line. Writes to a
/// <see cref="TextWriter"/> (defaults to <see cref="Console.Out"/>) purely
/// so tests can capture the output without touching the real console.
/// </summary>
public sealed class ConsolePayslipSubscriber : ISubscriber<PayslipResult>
{
    private readonly TextWriter _writer;

    public ConsolePayslipSubscriber(TextWriter? writer = null)
    {
        _writer = writer ?? Console.Out;
    }

    public void OnMessage(PayslipResult message)
    {
        var arrow = message.Trend switch
        {
            PayTrend.Increased => "^",
            PayTrend.Decreased => "v",
            _ => "="
        };

        _writer.WriteLine(
            $"[{message.PayPeriodUtc:yyyy-MM}] {message.EmployeeId,-8} Gross {message.GrossPay,9:N2} EPF {message.EpfContribution,8:N2} PCB {message.Pcb,8:N2} Net {message.NetPay,9:N2} {arrow} {message.ChangePercent,6:+0.00;-0.00;0.00}%");
    }
}
