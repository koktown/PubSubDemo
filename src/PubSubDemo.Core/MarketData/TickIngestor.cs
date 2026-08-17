using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.MarketData;

/// <summary>
/// Wires the transform step to the transport step. This is the only class
/// in the solution that knows both "how to turn a RawTick into a
/// PriceUpdate" and "how to broadcast a PriceUpdate" - everything on either
/// side of it depends only on interfaces and stays decoupled.
/// </summary>
public sealed class TickIngestor
{
    private readonly IDataTransformer<RawTick, PriceUpdate> _transformer;
    private readonly IPublisher<PriceUpdate> _publisher;

    public TickIngestor(IDataTransformer<RawTick, PriceUpdate> transformer, IPublisher<PriceUpdate> publisher)
    {
        _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public void Ingest(RawTick tick)
    {
        var update = _transformer.Transform(tick);
        _publisher.Publish(update);
    }
}
