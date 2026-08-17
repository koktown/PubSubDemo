using PubSubDemo.Core.MarketData;
using Xunit;

namespace PubSubDemo.Core.Tests.MarketData;

public class ConsoleTickerSubscriberTests
{
    [Fact]
    public void OnMessage_WritesSymbolAndChangePercentToTheWriter()
    {
        var writer = new StringWriter();
        var subscriber = new ConsoleTickerSubscriber(writer);
        var update = new PriceUpdate("ACME", 101.5m, 100m, 1.5m, Trend.Up, DateTimeOffset.UtcNow);

        subscriber.OnMessage(update);

        var output = writer.ToString();
        Assert.Contains("ACME", output);
        Assert.Contains("1.50", output);
    }
}
