using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.MarketData;

/// <summary>
/// Displays every price update as a single line. Writes to a
/// <see cref="TextWriter"/> (defaults to <see cref="Console.Out"/>) purely
/// so tests can capture the output without touching the real console.
/// </summary>
public sealed class ConsoleTickerSubscriber : ISubscriber<PriceUpdate>
{
    private readonly TextWriter _writer;

    public ConsoleTickerSubscriber(TextWriter? writer = null)
    {
        _writer = writer ?? Console.Out;
    }

    public void OnMessage(PriceUpdate message)
    {
        var arrow = message.Trend switch
        {
            Trend.Up => "^",
            Trend.Down => "v",
            _ => "="
        };

        _writer.WriteLine(
            $"[{message.TimestampUtc:HH:mm:ss}] {message.Symbol,-6} ${message.Price,7:N2} {arrow} {message.ChangePercent,6:+0.00;-0.00;0.00}%");
    }
}
