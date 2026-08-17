using PubSubDemo.Core.MarketData;

namespace PubSubDemo.App;

/// <summary>
/// Generates a small, deterministic (seeded) stream of ticks so the demo
/// is reproducible. Stands in for "read from a real feed" - nothing else
/// in the solution cares where RawTicks come from.
/// </summary>
internal static class SampleTickSource
{
    private static readonly string[] Symbols = { "ACME", "GLBX", "NDAQ" };

    public static IEnumerable<RawTick> Generate(int symbolCount, int ticksPerSymbol, int seed)
    {
        var random = new Random(seed);
        var symbols = Symbols.Take(symbolCount).ToArray();
        var prices = symbols.ToDictionary(s => s, _ => 100m + (decimal)random.NextDouble() * 50m);

        for (var round = 0; round < ticksPerSymbol; round++)
        {
            foreach (var symbol in symbols)
            {
                var drift = (decimal)(random.NextDouble() - 0.5) * 4m;
                prices[symbol] = Math.Max(1m, prices[symbol] + drift);

                yield return new RawTick(symbol, Math.Round(prices[symbol], 2), DateTimeOffset.UtcNow);
            }
        }
    }
}
