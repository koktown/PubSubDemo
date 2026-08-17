using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.MarketData;

/// <summary>
/// A second, independent subscriber that only reacts to significant price
/// moves. It demonstrates that each subscriber can apply its own filtering
/// or logic without affecting the publisher or any other subscriber - one
/// of the main selling points of the pattern.
/// </summary>
public sealed class AlertSubscriber : ISubscriber<PriceUpdate>
{
    private readonly decimal _thresholdPercent;
    private readonly TextWriter _writer;

    public AlertSubscriber(decimal thresholdPercent = 2.0m, TextWriter? writer = null)
    {
        if (thresholdPercent <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(thresholdPercent), "Threshold must be positive.");
        }

        _thresholdPercent = thresholdPercent;
        _writer = writer ?? Console.Out;
    }

    public void OnMessage(PriceUpdate message)
    {
        if (Math.Abs(message.ChangePercent) < _thresholdPercent)
        {
            return;
        }

        _writer.WriteLine($"ALERT: {message.Symbol} moved {message.ChangePercent:+0.00;-0.00}% to ${message.Price:N2}");
    }
}
