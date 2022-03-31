using System.Globalization;
using NUnit.Framework;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Services;

namespace TimePlanner.Domain.UnitTests.Services
{
  public class RecurrenceServiceTests
  {
    private RecurrenceService recurrenceService;
    // MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK
    const string TimeFormat = "dd/MM/yyyy HH:mm";
    static string TimeString(DateTime time) => time.ToString(TimeFormat, CultureInfo.InvariantCulture);
    static DateTime Time(string str) => DateTime.ParseExact(str, TimeFormat, CultureInfo.InvariantCulture);

    private void RunTest(string relateveTo, string cronExpression, string expected)
    {
      DateTime? next = recurrenceService.CalculateNextTime(cronExpression, Time(relateveTo));
      Assert.IsTrue(next.HasValue);
      Assert.AreEqual(expected, TimeString(next.Value));
    }

    [SetUp]
    public void SetUp()
    {
      recurrenceService = new RecurrenceService();
    }

    [TestCase("every sunday", "31/03/2022 14:30", "0 0 * * 0", "03/04/2022 00:00")]
    [TestCase("every sunday", "31/03/2022 14:30", "0 0 ? * 0", "03/04/2022 00:00")]
    [TestCase("every saturday", "31/03/2022 01:05", "0 0 * * 6", "02/04/2022 00:00")]
    [TestCase("every saturday", "31/03/2022 01:05", "0 0 ? * 6", "02/04/2022 00:00")]
    [TestCase("every monday", "31/03/2022 01:05", "0 0 * * 1", "04/04/2022 00:00")]
    [TestCase("every monday", "31/03/2022 01:05", "0 0 * ? 1", "04/04/2022 00:00")]
    public void TestWeekDays(string testName, string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [TestCase("", "31/03/2016 00:00", "0 0 31W * *", "31/03/2016 00:00")]
    [TestCase("", "31/03/2016 00:00", "0 0 31W * ?", "31/03/2016 00:00")]
    [TestCase("", "01/01/2016 00:00", "0 0 31W * *", "29/01/2016 00:00")]
    [TestCase("", "01/01/2016 00:00", "0 0 31W * ?", "29/01/2016 00:00")]
    [TestCase("", "01/02/2016 00:00", "0 0 31W * *", "31/03/2016 00:00")]
    [TestCase("", "01/02/2016 00:00", "0 0 31W * ?", "31/03/2016 00:00")]
    [TestCase("", "01/02/2016 00:00", "0 0 30W * *", "30/03/2016 00:00")]
    [TestCase("", "01/02/2016 00:00", "0 0 30W * ?", "30/03/2016 00:00")]
    [TestCase("", "01/02/2016 00:00", "0 0 29W * *", "29/02/2016 00:00")]
    [TestCase("", "01/02/2016 00:00", "0 0 29W * ?", "29/02/2016 00:00")]
    [TestCase("", "01/02/2017 00:00", "0 0 29W * *", "29/03/2017 00:00")]
    [TestCase("", "01/02/2017 00:00", "0 0 29W * ?", "29/03/2017 00:00")]
    public void TestEndOfMonths(string testName, string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [Test]
    public void TestWeekDays()
    {
    }
  }
}
