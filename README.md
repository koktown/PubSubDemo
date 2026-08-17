# Publish/Subscribe Demo — Market Ticker

A small, deliberately scoped solution demonstrating the Publish/Subscribe pattern in pure C#
(.NET 8). It simulates a market data feed: raw price ticks come in, get transformed into an
enriched update, and are broadcast to independent subscribers that each display the data
differently.

## Running it

```bash
dotnet run --project src/PubSubDemo.App
dotnet test tests/PubSubDemo.Core.Tests
```

No third-party runtime dependencies — the production code (`src/`) only uses the base class
library. The test project uses xUnit, restored from NuGet in the usual way.

## The pipeline

```
RawTick                    PriceUpdate                    subscribers
(input)                    (transformed)                  (display)

┌────────────┐   Transform   ┌──────────────┐   Publish   ┌───────────────────────┐
│ SampleTick  │ ───────────▶ │ PriceUpdate   │ ──────────▶ │ InMemoryEventBus<T>    │
│ Source      │              │ Transformer   │             │ (broker / transport)   │
└────────────┘               └──────────────┘             └───────────┬────────────┘
                                     ▲                                 │ fan-out
                              TickIngestor                             ▼
                              (glues the two                ┌─────────────────────┐
                               steps together)               │ ConsoleTickerSub.   │
                                                              │ AlertSubscriber     │
                                                              │ ...more, no changes  │
                                                              │ needed elsewhere     │
                                                              └─────────────────────┘
```

| Requirement                         | Where it lives                                             |
|--------------------------------------|-------------------------------------------------------------|
| Take an input of data                | `RawTick` + `SampleTickSource` (App project)                 |
| Transform that data                  | `PriceUpdateTransformer : IDataTransformer<RawTick, PriceUpdate>` |
| Transport to a set of subscribers    | `InMemoryEventBus<T> : IPublisher<T>, ISubscribable<T>`      |
| Subscribers display the data         | `ConsoleTickerSubscriber`, `AlertSubscriber`                 |

`TickIngestor` is the only class that knows about both the transform step and the publish
step; everything else only depends on interfaces (`IDataTransformer`, `IPublisher`,
`ISubscribable`, `ISubscriber`).

## Project layout

```
src/PubSubDemo.Core/
  Abstractions/        ISubscriber<T>, IPublisher<T>, ISubscribable<T>, IDataTransformer<TIn,TOut>
  Messaging/            InMemoryEventBus<T>  – the broker
  MarketData/            RawTick, PriceUpdate, Trend, PriceUpdateTransformer,
                          TickIngestor, ConsoleTickerSubscriber, AlertSubscriber
src/PubSubDemo.App/     Program.cs – wires everything together and runs a simulated stream
tests/PubSubDemo.Core.Tests/   one test class per production class, plus small hand-rolled test doubles
```

## Design choices worth calling out

- **Bus is generic and reusable.** `InMemoryEventBus<T>` doesn't know anything about market
  data — it would work identically for order events, log lines, or anything else. Swapping it
  for a real broker (Azure Service Bus, RabbitMQ, Kafka, SignalR) later only means writing a
  new `IPublisher<T>` / `ISubscribable<T>` implementation; nothing that depends on those
  interfaces has to change.
- **`Subscribe` returns `IDisposable`.** Idiomatic .NET (same shape as `IObservable<T>`),
  makes unsubscribing explicit, and plays nicely with `using` for scoped subscriptions.
- **Publish takes a snapshot of subscribers before iterating.** A subscriber that
  subscribes/unsubscribes *during* delivery can't corrupt the in-flight broadcast or deadlock
  against the lock.
- **One subscriber's exception can't break delivery to the others.** Faults are isolated and
  reported through an optional callback rather than propagating — a genuine concern once you
  have more than one subscriber and don't want a bug in the alerting logic to also kill the
  audit log.
- **Transformer is stateful but self-contained.** It tracks last price per symbol, but that
  state never leaks outside the class, so it's trivial to test without touching the bus at all.

## Testability

Every piece is tested in isolation, using plain hand-rolled fakes rather than a mocking
framework (deliberate — the seams are simple enough that a mocking library would be
overhead, not clarity):

- `PriceUpdateTransformerTests` — pure input → output assertions, no bus involved.
- `InMemoryEventBusTests` — fan-out to multiple subscribers, unsubscribe via `Dispose`,
  fault isolation when one subscriber throws.
- `TickIngestorTests` — a stub transformer and a recording publisher prove the ingestor calls
  one then the other, without needing a real transform or a real bus.
- `ConsoleTickerSubscriberTests` / `AlertSubscriberTests` — inject a `StringWriter` instead of
  `Console.Out` so display output can be asserted without touching the real console.

## Talking points for the interview

**What Pub/Sub buys you:** publishers and subscribers only share a message shape, not a
reference to each other. You can add `AlertSubscriber` without touching `ConsoleTickerSubscriber`
or the ingestor. You can run subscribers in parallel, add/remove them at runtime, or move the
transport out-of-process, all without touching the producer.

**What it costs:** a single synchronous in-memory bus like this one processes subscribers in
sequence — a slow subscriber delays the others and eventually the publisher, unless you delegate
to something async. Debugging is harder than a direct call chain, because "who handles this
message" isn't visible at the call site — you have to know the subscriber list. There's no
guaranteed delivery, ordering, or replay here; if that matters, that's exactly the kind of
thing a real broker (and much more code) buys you.

**Extension points worth discussing live:**
- Async subscribers (`Task OnMessageAsync(T message)`) for I/O-bound work, with a choice
  between fire-and-forget, `Task.WhenAll`, or a bounded queue per subscriber.
- Swapping `InMemoryEventBus<T>` for a real broker behind the same interfaces.
- Topic/type-based routing if subscribers only care about a subset of messages.
- Backpressure/buffering if a subscriber is consistently slower than the publish rate.
- Wiring this up through DI (`Microsoft.Extensions.DependencyInjection`) instead of the
  manual composition in `Program.cs`, once there's more than a couple of components.

I kept the demo itself deliberately small — happy to sketch any of the above on a whiteboard.
