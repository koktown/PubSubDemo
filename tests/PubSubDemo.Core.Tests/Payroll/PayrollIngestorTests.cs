using PubSubDemo.Core.Abstractions;
using PubSubDemo.Core.Payroll;
using PubSubDemo.Core.Tests.TestDoubles;
using Xunit;

namespace PubSubDemo.Core.Tests.Payroll;

public class PayrollIngestorTests
{
    [Fact]
    public void Ingest_TransformsTheEntryThenPublishesTheResult()
    {
        var expected = new PayslipResult("EMP001", 5000m, 550m, 0m, 4450m, null, 0m, PayTrend.Unchanged, DateTimeOffset.UtcNow);
        var transformer = new StubTransformer(expected);
        var publisher = new RecordingPublisher<PayslipResult>();
        var ingestor = new PayrollIngestor(transformer, publisher);
        var entry = new RawPayrollEntry("EMP001", 5000m, DateTimeOffset.UtcNow);

        ingestor.Ingest(entry);

        Assert.Same(entry, transformer.LastInput);
        Assert.Equal(new[] { expected }, publisher.Published);
    }

    private sealed class StubTransformer : IDataTransformer<RawPayrollEntry, PayslipResult>
    {
        private readonly PayslipResult _result;

        public StubTransformer(PayslipResult result) => _result = result;

        public RawPayrollEntry? LastInput { get; private set; }

        public PayslipResult Transform(RawPayrollEntry input)
        {
            LastInput = input;
            return _result;
        }
    }
}
