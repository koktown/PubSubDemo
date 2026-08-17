using PubSubDemo.Core.Abstractions;
using PubSubDemo.Core.MarketData;
using PubSubDemo.Core.Tests.TestDoubles;
using Xunit;

namespace PubSubDemo.Core.Tests.MarketData;

public class TickIngestorTests
{
    [Fact]
    public void Ingest_TransformsTheTickThenPublishesTheResult()
    {
        var expected = new PriceUpdate("ACME", 100m, null, 0m, Trend.Flat, DateTimeOffset.UtcNow);
        var transformer = new StubTransformer(expected);
        var publisher = new RecordingPublisher<PriceUpdate>();
        var ingestor = new TickIngestor(transformer, publisher);
        var tick = new RawTick("ACME", 100m, DateTimeOffset.UtcNow);

        ingestor.Ingest(tick);

        Assert.Same(tick, transformer.LastInput);
        Assert.Equal(new[] { expected }, publisher.Published);
    }

    private sealed class StubTransformer : IDataTransformer<RawTick, PriceUpdate>
    {
        private readonly PriceUpdate _result;

        public StubTransformer(PriceUpdate result) => _result = result;

        public RawTick? LastInput { get; private set; }

        public PriceUpdate Transform(RawTick input)
        {
            LastInput = input;
            return _result;
        }
    }
}
