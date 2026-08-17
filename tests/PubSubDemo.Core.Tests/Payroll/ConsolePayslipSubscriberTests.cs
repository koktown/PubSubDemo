using PubSubDemo.Core.Payroll;
using Xunit;

namespace PubSubDemo.Core.Tests.Payroll;

public class ConsolePayslipSubscriberTests
{
    [Fact]
    public void OnMessage_WritesEmployeeAndNetPayToTheWriter()
    {
        var writer = new StringWriter();
        var subscriber = new ConsolePayslipSubscriber(writer);
        var payslip = new PayslipResult("EMP001", 900m, 99m, 15m, 786m, 750m, 4.80m, PayTrend.Increased, DateTimeOffset.UtcNow);

        subscriber.OnMessage(payslip);

        var output = writer.ToString();
        Assert.Contains("EMP001", output);
        Assert.Contains("786.00", output);
    }
}
