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
      var rec = new Recurrence(Guid.NewGuid(), baseDate, "", null, null, null);
      Assert.AreEqual(baseDate.AddDays(1), recurrenceService.CalculateNextTime(rec));
    }
  }
}
