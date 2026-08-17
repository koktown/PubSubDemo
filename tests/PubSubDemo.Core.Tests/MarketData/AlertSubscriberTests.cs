using PubSubDemo.Core.MarketData;
using Xunit;

namespace PubSubDemo.Core.Tests.MarketData;

public class AlertSubscriberTests
{
    [Fact]
    public void OnMessage_WithChangeBelowThreshold_WritesNothing()
    {
        var writer = new StringWriter();
        var subscriber = new AlertSubscriber(thresholdPercent: 2m, writer: writer);
        var update = new PriceUpdate("ACME", 101m, 100m, 1.0m, Trend.Up, DateTimeOffset.UtcNow);

        subscriber.OnMessage(update);

        Assert.Equal(string.Empty, writer.ToString());
    }

    [Fact]
    public void OnMessage_WithChangeAtOrAboveThreshold_WritesAnAlert()
    {
        var writer = new StringWriter();
        var subscriber = new AlertSubscriber(thresholdPercent: 2m, writer: writer);
        var update = new PriceUpdate("ACME", 103m, 100m, 3.0m, Trend.Up, DateTimeOffset.UtcNow);

        subscriber.OnMessage(update);

        var output = writer.ToString();
        Assert.Contains("ALERT", output);
        Assert.Contains("ACME", output);
    }

    [Fact]
    public void Constructor_WithZeroThreshold_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AlertSubscriber(0m));
    }

    [Fact]
    public void Constructor_WithNegativeThreshold_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AlertSubscriber(-1m));
    }
}
