using System.Globalization;
using NUnit.Framework;
using TimePlanner.Domain.Services;

namespace TimePlanner.Domain.UnitTests.Services
{
  public class RecurrenceServiceTests
  {
    const string TimeFormat = "dd/MM/yyyy HH:mm";

    private RecurrenceService recurrenceService;

    private static string TimeString(DateTime time) =>
      time.ToString(TimeFormat, CultureInfo.InvariantCulture);

    private static DateTime Time(string str) =>
      DateTime.ParseExact(str, TimeFormat, CultureInfo.InvariantCulture);
    
    private void RunTest(string relativeTo, string cronExpression, string expected)
    {
      DateTime? next = recurrenceService.CalculateNextTime(cronExpression, Time(relativeTo));
      Assert.IsTrue(next.HasValue);
      Assert.AreEqual(expected, TimeString(next.Value));
    }

    [SetUp]
    public void SetUp()
    {
      recurrenceService = new RecurrenceService();
    }

    [TestCase("31/03/2022 14:30", "0 0 * * 0", "03/04/2022 00:00")]
    [TestCase("31/03/2022 14:30", "0 0 ? * 0", "03/04/2022 00:00")]
    [TestCase("31/03/2022 01:05", "0 0 * * 6", "02/04/2022 00:00")]
    [TestCase("31/03/2022 01:05", "0 0 ? * 6", "02/04/2022 00:00")]
    [TestCase("31/03/2022 01:05", "0 0 * * 1", "04/04/2022 00:00")]
    [TestCase("31/03/2022 01:05", "0 0 * ? 1", "04/04/2022 00:00")]
    public void TestWeekDays(string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [TestCase("31/03/2022 00:00", "0 0 31W * *", "31/05/2022 00:00")]
    [TestCase("31/03/2022 00:00", "0 0 31W * ?", "31/05/2022 00:00")]
    [TestCase("01/01/2016 00:00", "0 0 31W * *", "29/01/2016 00:00")]
    [TestCase("01/01/2016 00:00", "0 0 31W * ?", "29/01/2016 00:00")]
    [TestCase("01/02/2016 00:00", "0 0 31W * *", "31/03/2016 00:00")]
    [TestCase("01/02/2016 00:00", "0 0 31W * ?", "31/03/2016 00:00")]
    [TestCase("01/02/2016 00:00", "0 0 30W * *", "30/03/2016 00:00")]
    [TestCase("01/02/2016 00:00", "0 0 30W * ?", "30/03/2016 00:00")]
    [TestCase("01/02/2016 00:00", "0 0 29W * *", "29/02/2016 00:00")]
    [TestCase("01/02/2016 00:00", "0 0 29W * ?", "29/02/2016 00:00")]
    [TestCase("01/02/2017 00:00", "0 0 29W * *", "29/03/2017 00:00")]
    [TestCase("01/02/2017 00:00", "0 0 29W * ?", "29/03/2017 00:00")]
    public void TestWeekDaysAtEndOfMonths(string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [TestCase("01/01/2016 00:00", "* * ? * *", "01/01/2016 00:01")]
    [TestCase("01/01/2016 00:01", "* * * * ?", "01/01/2016 00:02")]
    [TestCase("01/01/2016 00:02", "* * ? * ?", "01/01/2016 00:03")]
    [TestCase("01/01/2003 00:00", "* * * * *", "01/01/2003 00:01")]
    [TestCase("01/01/2003 00:01", "* * * * *", "01/01/2003 00:02")]
    [TestCase("01/01/2003 00:02", "* * * * *", "01/01/2003 00:03")]
    [TestCase("01/01/2003 00:58", "* * * * *", "01/01/2003 00:59")]
    [TestCase("01/01/2003 01:58", "* * * * *", "01/01/2003 01:59")]
    [TestCase("01/01/2003 00:59", "* * * * *", "01/01/2003 01:00")]
    [TestCase("01/01/2003 01:59", "* * * * *", "01/01/2003 02:00")]
    [TestCase("01/01/2003 23:59", "* * * * *", "02/01/2003 00:00")]
    [TestCase("31/12/2003 23:59", "* * * * *", "01/01/2004 00:00")]
    [TestCase("28/02/2003 23:59", "* * * * *", "01/03/2003 00:00")]
    [TestCase("28/02/2004 23:59", "* * * * *", "29/02/2004 00:00")]
    public void TestEveryDay(string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [TestCase("07/01/2003 00:00", "30  *  1 * *", "01/02/2003 00:30")]
    [TestCase("01/02/2003 00:30", "30  *  1 * *", "01/02/2003 01:30")]
    [TestCase("01/01/2003 00:00", "10  * 22    * *", "22/01/2003 00:10")]
    [TestCase("01/01/2003 00:00", "30 23 19    * *", "19/01/2003 23:30")]
    [TestCase("01/01/2003 00:00", "30 23 21    * *", "21/01/2003 23:30")]
    [TestCase("01/01/2003 00:01", " *  * 21    * *", "21/01/2003 00:00")]
    [TestCase("10/07/2003 00:00", " *  * 30,31 * *", "30/07/2003 00:00")]
    [TestCase("20/01/2016 00:00", " *  * 1W * *", "01/02/2016 00:00")]
    [TestCase("28/04/2016 00:00", " *  * 1W * *", "02/05/2016 00:00")]
    [TestCase("30/09/2016 00:00", " *  * 1W * *", "03/10/2016 00:00")]
    [TestCase("01/02/2003 00:00", " *  * 15W * *", "14/02/2003 00:00")]
    [TestCase("01/06/2003 00:00", " *  * 15W * *", "16/06/2003 00:00")]
    [TestCase("10/08/2003 00:00", " *  * LW * *", "29/08/2003 00:00")]
    [TestCase("10/10/2015 00:00", " *  * LW * *", "30/10/2015 00:00")]
    [TestCase("10/07/2003 00:00", " *  * L * *", "31/07/2003 00:00")]
    [TestCase("01/02/2015 00:00", " *  * L * *", "28/02/2015 00:00")]
    [TestCase("01/02/2016 00:00", " *  * L * *", "29/02/2016 00:00")]
    public void TestDayOfMonth(string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [TestCase("28/02/2002 23:59", "* * * 3 *", "01/03/2002 00:00")]
    [TestCase("29/02/2004 23:59", "* * * 3 *", "01/03/2004 00:00")]
    [TestCase("31/03/2002 23:59", "* * * 4 *", "01/04/2002 00:00")]
    [TestCase("30/04/2002 23:59", "* * * 5 *", "01/05/2002 00:00")]
    public void TestMonthRollovers(string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [TestCase("01/01/2000 00:00", "0 0 15,30,31 * *", "15/01/2000 00:00")]
    [TestCase("15/01/2000 00:00", "0 0 15,30,31 * *", "30/01/2000 00:00")]
    [TestCase("30/01/2000 00:00", "0 0 15,30,31 * *", "31/01/2000 00:00")]
    [TestCase("31/01/2000 00:00", "0 0 15,30,31 * *", "15/02/2000 00:00")]

    [TestCase("15/02/2000 00:00", "0 0 15,30,31 * *", "15/03/2000 00:00")]

    [TestCase("15/03/2000 00:00", "0 0 15,30,31 * *", "30/03/2000 00:00")]
    [TestCase("30/03/2000 00:00", "0 0 15,30,31 * *", "31/03/2000 00:00")]
    [TestCase("31/03/2000 00:00", "0 0 15,30,31 * *", "15/04/2000 00:00")]

    [TestCase("15/04/2000 00:00", "0 0 15,30,31 * *", "30/04/2000 00:00")]
    [TestCase("30/04/2000 00:00", "0 0 15,30,31 * *", "15/05/2000 00:00")]

    [TestCase("15/05/2000 00:00", "0 0 15,30,31 * *", "30/05/2000 00:00")]
    [TestCase("30/05/2000 00:00", "0 0 15,30,31 * *", "31/05/2000 00:00")]
    [TestCase("31/05/2000 00:00", "0 0 15,30,31 * *", "15/06/2000 00:00")]

    [TestCase("15/06/2000 00:00", "0 0 15,30,31 * *", "30/06/2000 00:00")]
    [TestCase("30/06/2000 00:00", "0 0 15,30,31 * *", "15/07/2000 00:00")]

    [TestCase("15/07/2000 00:00", "0 0 15,30,31 * *", "30/07/2000 00:00")]
    [TestCase("30/07/2000 00:00", "0 0 15,30,31 * *", "31/07/2000 00:00")]
    [TestCase("31/07/2000 00:00", "0 0 15,30,31 * *", "15/08/2000 00:00")]

    [TestCase("15/08/2000 00:00", "0 0 15,30,31 * *", "30/08/2000 00:00")]
    [TestCase("30/08/2000 00:00", "0 0 15,30,31 * *", "31/08/2000 00:00")]
    [TestCase("31/08/2000 00:00", "0 0 15,30,31 * *", "15/09/2000 00:00")]

    [TestCase("15/09/2000 00:00", "0 0 15,30,31 * *", "30/09/2000 00:00")]
    [TestCase("30/09/2000 00:00", "0 0 15,30,31 * *", "15/10/2000 00:00")]

    [TestCase("15/10/2000 00:00", "0 0 15,30,31 * *", "30/10/2000 00:00")]
    [TestCase("30/10/2000 00:00", "0 0 15,30,31 * *", "31/10/2000 00:00")]
    [TestCase("31/10/2000 00:00", "0 0 15,30,31 * *", "15/11/2000 00:00")]

    [TestCase("15/11/2000 00:00", "0 0 15,30,31 * *", "30/11/2000 00:00")]
    [TestCase("30/11/2000 00:00", "0 0 15,30,31 * *", "15/12/2000 00:00")]

    [TestCase("15/12/2000 00:00", "0 0 15,30,31 * *", "30/12/2000 00:00")]
    [TestCase("30/12/2000 00:00", "0 0 15,30,31 * *", "31/12/2000 00:00")]
    [TestCase("31/12/2000 00:00", "0 0 15,30,31 * *", "15/01/2001 00:00")]
    public void TestMonth30_31(string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [TestCase("01/12/2003 05:00", "10 * * 6 *", "01/06/2004 00:10")]
    [TestCase("04/01/2003 00:00", " 1 2 3 * *", "03/02/2003 02:01")]
    [TestCase("01/01/2003 00:00", "0 12 1 6 *", "01/06/2003 12:00")]
    [TestCase("11/09/1988 14:23", "* 12 1 6 *", "01/06/1989 12:00")]
    [TestCase("11/03/1988 14:23", "* 12 1 6 *", "01/06/1988 12:00")]
    [TestCase("11/03/1988 14:23", "* 2,4-8,15 * 6 *", "01/06/1988 02:00")]
    [TestCase("11/03/1988 14:23", "20 * * jan,FeB,Mar,apr,May,JuN,Jul,Aug,Sep-Nov,DEC *", "11/03/1988 15:20")]
    public void TestMonths(string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [TestCase("26/06/2003 10:00", "30 6 * * 0", "29/06/2003 06:30")]
    [TestCase("26/06/2003 10:00", "30 6 * * sun", "29/06/2003 06:30")]
    [TestCase("26/06/2003 10:00", "30 6 * * SUN", "29/06/2003 06:30")]
    [TestCase("19/06/2003 00:00", "1 12 * * 2", "24/06/2003 12:01")]
    [TestCase("24/06/2003 12:01", "1 12 * * 2", "01/07/2003 12:01")]

    [TestCase("01/06/2003 14:55", "15 18 * * Mon", "02/06/2003 18:15")]
    [TestCase("02/06/2003 18:15", "15 18 * * Mon", "09/06/2003 18:15")]
    [TestCase("09/06/2003 18:15", "15 18 * * Mon", "16/06/2003 18:15")]
    [TestCase("16/06/2003 18:15", "15 18 * * Mon", "23/06/2003 18:15")]
    [TestCase("23/06/2003 18:15", "15 18 * * Mon", "30/06/2003 18:15")]
    [TestCase("30/06/2003 18:15", "15 18 * * Mon", "07/07/2003 18:15")]

    [TestCase("01/01/2003 00:00", "* * * * Mon", "06/01/2003 00:00")]
    [TestCase("01/01/2003 12:00", "45 16 1 * Mon", "01/09/2003 16:45")]
    [TestCase("01/09/2003 23:45", "45 16 1 * Mon", "01/12/2003 16:45")]

    [TestCase("01/09/2003 23:45", "45 16 * * Mon#2", "08/09/2003 16:45")]
    [TestCase("01/09/2003 23:45", "45 16 * * 2#4", "23/09/2003 16:45")]

    [TestCase("01/01/2003 23:45", "0 0 * * 0L", "26/01/2003 00:00")]
    [TestCase("01/01/2003 23:45", "0 0 * * SUNL", "26/01/2003 00:00")]
    [TestCase("01/01/2003 23:45", "0 0 * * 6L", "25/01/2003 00:00")]
    [TestCase("01/01/2003 23:45", "0 0 * * SATL", "25/01/2003 00:00")]
    public void TestDayOfWeek(string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [TestCase("01/01/2000 12:00", "1 12 29 2 *", "29/02/2000 12:01")]
    [TestCase("29/02/2000 12:01", "1 12 29 2 *", "29/02/2004 12:01")]
    [TestCase("29/02/2004 12:01", "1 12 29 2 *", "29/02/2008 12:01")]
    public void LeapYearTests(string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [TestCase("01/01/2000 12:00", "1 12 28 2 *", "28/02/2000 12:01")]
    [TestCase("28/02/2000 12:01", "1 12 28 2 *", "28/02/2001 12:01")]
    [TestCase("28/02/2001 12:01", "1 12 28 2 *", "28/02/2002 12:01")]
    [TestCase("28/02/2002 12:01", "1 12 28 2 *", "28/02/2003 12:01")]
    [TestCase("28/02/2003 12:01", "1 12 28 2 *", "28/02/2004 12:01")]
    [TestCase("29/02/2004 12:01", "1 12 28 2 *", "28/02/2005 12:01")]
    public void NonLeapYearTests(string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }

    [TestCase("01/01/2003 00:00", "* * * * *", "01/01/2003 00:01")]
    [TestCase("31/12/2002 23:59", "* * * * *", "01/01/2003 00:00")]
    [TestCase("31/12/2002 23:59", "* * * * Mon", "06/01/2003 00:00")]
    [TestCase("01/01/2003 00:00", "* * * * Mon", "06/01/2003 00:00")]
    [TestCase("01/01/2003 00:00", "30 12 * * Mon", "06/01/2003 12:30")]
    public void TestFiniteOccurrences(string relativeTo, string cronExpression, string expected)
    {
      RunTest(relativeTo, cronExpression, expected);
    }
  }
}
