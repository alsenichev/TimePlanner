using Moq;
using NUnit.Framework;
using TimePlanner.Domain.Interfaces;
using TimePlanner.Domain.Models;
using TimePlanner.Domain.Services;

namespace TimePlanner.Domain.UnitTests.Services
{
  public class RecurrenceServiceTests
  {
    private static readonly DateTime baseDate = new DateTime(2022, 05, 10, 14, 05, 16);

    private RecurrenceService recurrenceService;
    private Mock<ITimeProvider> timeProviderMock;

    private Recurrence CreateRecurrence(
      int? YearsEveryN = null,
      List<int>? YearsCustom = null,
      int? MonthsEveryN = null,
      List<int>? MonthsCustom = null,
      int? WeeksEveryN = null,
      List<int>? WeeksCustom = null,
      List<int>? WeekDaysCustom = null,
      int? DaysEveryN = null,
      List<int>? DaysCustom = null,
      int? RepetitionCount = null,
      int? MaxRepetitionCount = null,
      bool? IsAfterPreviousCompleted = null)
    {
      return new Recurrence(
        Guid.NewGuid(),
        YearsEveryN,
        YearsCustom,
        MonthsEveryN,
        MonthsCustom,
        WeeksEveryN,
        WeeksCustom,
        WeekDaysCustom,
        DaysEveryN,
        DaysCustom,
        RepetitionCount,
        MaxRepetitionCount,
        IsAfterPreviousCompleted);
    }

    [SetUp]
    public void SetUp()
    {
      timeProviderMock = new Mock<ITimeProvider>();
      timeProviderMock.Setup(m => m.Now).Returns(baseDate);
      recurrenceService = new RecurrenceService(timeProviderMock.Object);
    }


    [Test]
    public void TestEveryDay()
    {
      var rec = CreateRecurrence(DaysEveryN: 1);
      Assert.AreEqual(baseDate.AddDays(1), recurrenceService.CalculateNextTime(rec));
    }
  }
}
