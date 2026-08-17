using PubSubDemo.App;
using PubSubDemo.Core.Messaging;
using PubSubDemo.Core.Payroll;

// 1) The broker. Generic, reusable, knows nothing about payroll.
var bus = new InMemoryEventBus<PayslipResult>(
    onSubscriberError: (subscriber, ex) =>
        Console.Error.WriteLine($"Subscriber {subscriber.GetType().Name} threw: {ex.Message}"));

// 2) The pipeline: raw input -> transform (EPF + PCB deductions) -> publish.
var ingestor = new PayrollIngestor(new PayslipCalculator(new SimplifiedMonthlyTaxCalculator()), bus);

// 3) Subscribers register independently of one another and of the ingestor.
using var payslips = bus.Subscribe(new ConsolePayslipSubscriber());
using var anomalyAlerts = bus.Subscribe(new PayrollAnomalyAlertSubscriber(thresholdPercent: 15m));

Console.WriteLine("Processing simulated payroll run (Ctrl+C to stop early)...\n");

foreach (var entry in SamplePayInputSource.Generate(employeeCount: 3, payPeriods: 4, seed: 42))
{
    ingestor.Ingest(entry);
    await Task.Delay(150);
}

Console.WriteLine("\nDone.");
