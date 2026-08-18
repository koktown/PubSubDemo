# Publish/Subscribe Demo — Payroll & Tax Processing

A small, deliberately scoped solution demonstrating the Publish/Subscribe pattern in pure C#
(.NET 8). It simulates a payroll run: raw payroll entries come in, get transformed into a
payslip (gross pay, EPF, PCB, net pay), and are broadcast to independent subscribers that each
do something different with the result.

## Running it

```bash
dotnet run --project src/PubSubDemo.App
dotnet test tests/PubSubDemo.Core.Tests
```

No third-party runtime dependencies — the production code (`src/`) only uses the base class
library. The test project uses xUnit, restored from NuGet in the usual way.

## The pipeline

```
RawPayrollEntry            PayslipResult                  subscribers
(input)                    (transformed)                  (display)

┌────────────┐   Transform   ┌──────────────┐   Publish   ┌───────────────────────┐
│ SamplePay   │ ───────────▶ │ Payslip       │ ──────────▶ │ InMemoryEventBus<T>    │
│ InputSource │              │ Calculator    │             │ (broker / transport)   │
└────────────┘               └──────┬───────┘             └───────────┬────────────┘
                                     │ delegates                       │ fan-out
                              ITaxCalculator                           ▼
                              (EPF is inline,                ┌─────────────────────┐
                               PCB is its own seam)           │ ConsolePayslipSub.   │
                                     ▲                        │ PayrollAnomalyAlert  │
                              PayrollIngestor                 │ Subscriber           │
                              (glues the two                  │ ...more, no changes  │
                               steps together)                 │ needed elsewhere    │
                                                                └─────────────────────┘
```

| Requirement                         | Where it lives                                             |
|--------------------------------------|-------------------------------------------------------------|
| Take an input of data                | `RawPayrollEntry` + `SamplePayInputSource` (App project)     |
| Transform that data                  | `PayslipCalculator : IDataTransformer<RawPayrollEntry, PayslipResult>`, delegating tax to `ITaxCalculator` |
| Transport to a set of subscribers    | `InMemoryEventBus<T> : IPublisher<T>, ISubscribable<T>`      |
| Subscribers display the data         | `ConsolePayslipSubscriber`, `PayrollAnomalyAlertSubscriber`  |

`PayrollIngestor` is the only class that knows about both the transform step and the publish
step; everything else only depends on interfaces (`IDataTransformer`, `IPublisher`,
`ISubscribable`, `ISubscriber`).

## Project layout

```
src/PubSubDemo.Core/
  Abstractions/        ISubscriber<T>, IPublisher<T>, ISubscribable<T>, IDataTransformer<TIn,TOut>
  Messaging/            InMemoryEventBus<T>  – the broker
  Payroll/               RawPayrollEntry, PayslipResult, PayTrend, ITaxCalculator,
                          SimplifiedMonthlyTaxCalculator, PayslipCalculator,
                          PayrollIngestor, ConsolePayslipSubscriber, PayrollAnomalyAlertSubscriber
src/PubSubDemo.App/     Program.cs – wires everything together and runs a simulated payroll run
tests/PubSubDemo.Core.Tests/   one test class per production class, plus small hand-rolled test doubles
```

## Design choices worth calling out

- **Bus is generic and reusable.** `InMemoryEventBus<T>` doesn't know anything about payroll —
  it would work identically for order events, log lines, or anything else. Swapping it for a
  real broker (Azure Service Bus, RabbitMQ, Kafka, SignalR) later only means writing a new
  `IPublisher<T>` / `ISubscribable<T>` implementation; nothing that depends on those interfaces
  has to change.
- **Tax calculation sits behind its own interface.** `PayslipCalculator` depends on
  `ITaxCalculator` rather than computing PCB itself. Tax rules are the part of a payroll system
  most likely to change — by jurisdiction, by tax year, by employee category — so that seam
  keeps the swap local to one implementation instead of touching the ingestor or the bus.
- **`Subscribe` returns `IDisposable`.** Idiomatic .NET (same shape as `IObservable<T>`),
  makes unsubscribing explicit, and plays nicely with `using` for scoped subscriptions.
- **Publish takes a snapshot of subscribers before iterating.** A subscriber that
  subscribes/unsubscribes *during* delivery can't corrupt the in-flight broadcast or deadlock
  against the lock.
- **One subscriber's exception can't break delivery to the others.** Faults are isolated and
  reported through an optional callback rather than propagating — a genuine concern once you
  have more than one subscriber and don't want a bug in the alerting logic to also kill the
  console output.
- **`PayslipCalculator` is stateful but self-contained.** It tracks last net pay per employee,
  but that state never leaks outside the class, so it's trivial to test without touching the
  bus at all.

## Testability

Every piece is tested in isolation, using plain hand-rolled fakes rather than a mocking
framework (deliberate — the seams are simple enough that a mocking library would be overhead,
not clarity):

- `PayslipCalculatorTests` — pure input → output assertions against a stub `ITaxCalculator`,
  no bus involved.
- `SimplifiedMonthlyTaxCalculatorTests` — the tax bands in isolation, independent of payslip
  calculation entirely.
- `InMemoryEventBusTests` — fan-out to multiple subscribers, unsubscribe via `Dispose`, fault
  isolation when one subscriber throws.
- `PayrollIngestorTests` — a stub transformer and a recording publisher prove the ingestor
  calls one then the other, without needing a real transform or a real bus.
- `ConsolePayslipSubscriberTests` / `PayrollAnomalyAlertSubscriberTests` — inject a
  `StringWriter` instead of `Console.Out` so display output can be asserted without touching
  the real console.
