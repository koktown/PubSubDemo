namespace PubSubDemo.Core.MarketData;

/// <summary>
/// The "output" shape - what a <see cref="RawTick"/> becomes after the
/// transform step has enriched it. This is what actually travels through
/// the bus and what every subscriber consumes.
/// </summary>
public sealed record PriceUpdate(
    string Symbol,
    decimal Price,
    decimal? PreviousPrice,
    decimal ChangePercent,
    Trend Trend,
    DateTimeOffset TimestampUtc);
