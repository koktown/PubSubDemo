using PubSubDemo.Core.Abstractions;

namespace PubSubDemo.Core.Payroll;

/// <summary>
/// Wires the transform step to the transport step. This is the only class
/// in the solution that knows both "how to turn a RawPayrollEntry into a
/// PayslipResult" and "how to broadcast a PayslipResult" - everything on
/// either side of it depends only on interfaces and stays decoupled.
/// </summary>
public sealed class PayrollIngestor
{
    private readonly IDataTransformer<RawPayrollEntry, PayslipResult> _transformer;
    private readonly IPublisher<PayslipResult> _publisher;

    public PayrollIngestor(IDataTransformer<RawPayrollEntry, PayslipResult> transformer, IPublisher<PayslipResult> publisher)
    {
        _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public void Ingest(RawPayrollEntry entry)
    {
        var result = _transformer.Transform(entry);
        _publisher.Publish(result);
    }
}
