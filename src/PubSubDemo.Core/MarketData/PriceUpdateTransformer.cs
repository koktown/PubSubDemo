using System.Collections.Concurrent;
using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.MarketData;

/// <summary>
/// Turns a raw tick into a <see cref="PriceUpdate"/> by comparing it against
/// the last price seen for that symbol. It is stateful (one "last price" per
/// symbol) but the state is fully encapsulated here - no bus, no console, no
/// subscribers - which is what makes it trivial to unit test on its own.
/// </summary>
public sealed class PriceUpdateTransformer : IDataTransformer<RawTick, PriceUpdate>
{
    private const decimal FlatThresholdPercent = 0.01m;

    private readonly ConcurrentDictionary<string, decimal> _lastPriceBySymbol = new();

    public PriceUpdate Transform(RawTick input)
    {
        ArgumentNullException.ThrowIfNull(input);

        decimal? previousPrice = _lastPriceBySymbol.TryGetValue(input.Symbol, out var last)
            ? last
            : null;

        var changePercent = previousPrice is > 0m
            ? Math.Round((input.Price - previousPrice.Value) / previousPrice.Value * 100m, 2)
            : 0m;

        var trend = changePercent switch
        {
            > FlatThresholdPercent => Trend.Up,
            < -FlatThresholdPercent => Trend.Down,
            _ => Trend.Flat
        };

        _lastPriceBySymbol[input.Symbol] = input.Price;

        return new PriceUpdate(input.Symbol, input.Price, previousPrice, changePercent, trend, input.TimestampUtc);
    }
}
