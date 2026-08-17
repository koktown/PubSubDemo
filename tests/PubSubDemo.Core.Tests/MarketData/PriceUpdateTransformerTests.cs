using PubSubDemo.Core.MarketData;
using Xunit;

namespace PubSubDemo.Core.Tests.MarketData;

public class PriceUpdateTransformerTests
{
    [Fact]
    public void Transform_FirstTickForSymbol_HasNoPreviousPriceAndZeroChange()
    {
        var transformer = new PriceUpdateTransformer();
        var tick = new RawTick("ACME", 100m, DateTimeOffset.UtcNow);

        var result = transformer.Transform(tick);

        Assert.Null(result.PreviousPrice);
        Assert.Equal(0m, result.ChangePercent);
        Assert.Equal(Trend.Flat, result.Trend);
    }

    [Fact]
    public void Transform_PriceIncrease_ReturnsPositiveChangeAndUpTrend()
    {
        var transformer = new PriceUpdateTransformer();
        transformer.Transform(new RawTick("ACME", 100m, DateTimeOffset.UtcNow));

        var result = transformer.Transform(new RawTick("ACME", 110m, DateTimeOffset.UtcNow));

        Assert.Equal(100m, result.PreviousPrice);
        Assert.Equal(10.00m, result.ChangePercent);
        Assert.Equal(Trend.Up, result.Trend);
    }

    [Fact]
    public void Transform_PriceDecrease_ReturnsNegativeChangeAndDownTrend()
    {
        var transformer = new PriceUpdateTransformer();
        transformer.Transform(new RawTick("ACME", 100m, DateTimeOffset.UtcNow));

        var result = transformer.Transform(new RawTick("ACME", 90m, DateTimeOffset.UtcNow));

        Assert.Equal(-10.00m, result.ChangePercent);
        Assert.Equal(Trend.Down, result.Trend);
    }

    [Fact]
    public void Transform_TracksEachSymbolIndependently()
    {
        var transformer = new PriceUpdateTransformer();
        transformer.Transform(new RawTick("ACME", 100m, DateTimeOffset.UtcNow));
        transformer.Transform(new RawTick("GLBX", 50m, DateTimeOffset.UtcNow));

        var result = transformer.Transform(new RawTick("GLBX", 55m, DateTimeOffset.UtcNow));

        Assert.Equal(50m, result.PreviousPrice);
        Assert.Equal(10.00m, result.ChangePercent);
    }
}
