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

    [SetUp]
    public void SetUp()
    {
      recurrenceService = new RecurrenceService();
    }

    [TestCase("every sunday", "31/03/2022 14:30", "0 0 * * 0", "03/04/2022 00:00")]
    [TestCase("every saturday", "31/03/2022 01:05", "0 0 * * 6", "02/04/2022 00:00")]
    [TestCase("every monday", "31/03/2022 01:05", "0 0 * * 1", "04/04/2022 00:00")]
    public void TestDays(string testName, string baseTime, string cron, string expected)
    {
      DateTime? next = recurrenceService.CalculateNextTime(cron, Time(baseTime));
      Assert.IsTrue(next.HasValue);
      Assert.AreEqual(expected, TimeString(next.Value));
    }

    [Test]
    public void TestMonths()
    {
    }

    [Test]
    public void TestWeekDays()
    {
    }
  }
}
