namespace PubSubDemo.Core.MarketData;

/// <summary>
/// The "input" shape - a single raw price observation exactly as it would
/// arrive from an external feed. Intentionally minimal; this is the wire
/// format, not what subscribers want to look at.
/// </summary>
public sealed record RawTick(string Symbol, decimal Price, DateTimeOffset TimestampUtc);
