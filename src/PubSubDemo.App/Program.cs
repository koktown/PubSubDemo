using PubSubDemo.App;
using PubSubDemo.Core.MarketData;
using PubSubDemo.Core.Messaging;

// 1) The broker. Generic, reusable, knows nothing about market data.
var bus = new InMemoryEventBus<PriceUpdate>(
    onSubscriberError: (subscriber, ex) =>
        Console.Error.WriteLine($"Subscriber {subscriber.GetType().Name} threw: {ex.Message}"));

// 2) The pipeline: raw input -> transform -> publish.
var ingestor = new TickIngestor(new PriceUpdateTransformer(), bus);

// 3) Subscribers register independently of one another and of the ingestor.
using var ticker = bus.Subscribe(new ConsoleTickerSubscriber());
using var alerts = bus.Subscribe(new AlertSubscriber(thresholdPercent: 1.5m));

Console.WriteLine("Streaming simulated ticks (Ctrl+C to stop early)...\n");

foreach (var tick in SampleTickSource.Generate(symbolCount: 3, ticksPerSymbol: 8, seed: 42))
{
    ingestor.Ingest(tick);
    await Task.Delay(150);
}

Console.WriteLine("\nDone.");
