using PubSubDemo.Core.Payroll;
using Xunit;

namespace PubSubDemo.Core.Tests.Payroll;

public class PayrollAnomalyAlertSubscriberTests
{
    [Fact]
    public void OnMessage_WithChangeBelowThreshold_WritesNothing()
    {
        var writer = new StringWriter();
        var subscriber = new PayrollAnomalyAlertSubscriber(thresholdPercent: 15m, writer: writer);
        var payslip = new PayslipResult("EMP001", 5000m, 550m, 0m, 4450m, 4400m, 1.0m, PayTrend.Increased, DateTimeOffset.UtcNow);

        subscriber.OnMessage(payslip);

        Assert.Equal(string.Empty, writer.ToString());
    }

    [Fact]
    public void OnMessage_WithChangeAtOrAboveThreshold_WritesAReviewFlag()
    {
        var writer = new StringWriter();
        var subscriber = new PayrollAnomalyAlertSubscriber(thresholdPercent: 15m, writer: writer);
        var payslip = new PayslipResult("EMP001", 8000m, 880m, 300m, 6820m, 4450m, 53.3m, PayTrend.Increased, DateTimeOffset.UtcNow);

        subscriber.OnMessage(payslip);

        var output = writer.ToString();
        Assert.Contains("REVIEW", output);
        Assert.Contains("EMP001", output);
    }

    [Fact]
    public void Constructor_WithZeroThreshold_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PayrollAnomalyAlertSubscriber(0m));
    }

    [Fact]
    public void Constructor_WithNegativeThreshold_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PayrollAnomalyAlertSubscriber(-1m));
    }
}
