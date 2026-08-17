using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.Payroll;

/// <summary>
/// A second, independent subscriber that only reacts to large swings in an
/// employee's net pay versus their last pay period - the kind of thing
/// worth a human double-check before a payroll run is submitted. It
/// demonstrates that each subscriber can apply its own filtering or logic
/// without affecting the publisher or any other subscriber - one of the
/// main selling points of the pattern.
/// </summary>
public sealed class PayrollAnomalyAlertSubscriber : ISubscriber<PayslipResult>
{
    private readonly decimal _thresholdPercent;
    private readonly TextWriter _writer;

    public PayrollAnomalyAlertSubscriber(decimal thresholdPercent = 15.0m, TextWriter? writer = null)
    {
        if (thresholdPercent <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(thresholdPercent), "Threshold must be positive.");
        }

        _thresholdPercent = thresholdPercent;
        _writer = writer ?? Console.Out;
    }

    public void OnMessage(PayslipResult message)
    {
        if (Math.Abs(message.ChangePercent) < _thresholdPercent)
        {
            return;
        }

        _writer.WriteLine($"REVIEW: {message.EmployeeId} net pay moved {message.ChangePercent:+0.00;-0.00}% to {message.NetPay:N2} - verify before submission.");
    }
}
