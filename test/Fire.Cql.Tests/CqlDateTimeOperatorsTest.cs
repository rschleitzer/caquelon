using Xunit;

namespace Fire.Cql.Tests;

public class CqlDateTimeOperatorsTest
{
    [Fact]
    public async Task Add()
    {
        Assert.True(await Helpers.CheckBool("DateTime(2005, 10, 10) + 5 years = @2010-10-10T")); // DateTimeAdd5Years
        Assert.True(await Helpers.CheckBool("DateTime(2005, 10, 10) + 8000 years = ")); // DateTimeAddInvalidYears
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10) + 5 months = @2005-10-10T")); // DateTimeAdd5Months
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10) + 10 months = @2006-03-10T")); // DateTimeAddMonthsOverflow
        Assert.True(await Helpers.CheckBool("DateTime(2018, 5, 2) + 3 weeks = DateTime(2018, 5, 23)")); // DateTimeAddThreeWeeks
        Assert.True(await Helpers.CheckBool("DateTime(2018, 5, 23) + 52 weeks = DateTime(2019, 5, 22)")); // DateTimeAddYearInWeeks
        Assert.True(await Helpers.CheckBool("DateTime(2023, 3, 2) + 52 weeks = DateTime(2024, 2, 29)")); // DateTimeLeapDayAddYearInWeeks
        Assert.True(await Helpers.CheckBool("DateTime(2024, 2, 28) + 52 weeks = DateTime(2025, 2, 26)")); // DateTimeLeapYearAddYearInWeeks
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10) + 5 days = @2005-05-15T")); // DateTimeAdd5Days
        Assert.True(await Helpers.CheckBool("DateTime(2016, 6, 10) + 21 days = @2016-07-01T")); // DateTimeAddDaysOverflow
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10, 5) + 5 hours = @2005-05-10T10")); // DateTimeAdd5Hours
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10, 5, 20, 30) + 5 hours = @2005-05-10T10:20:30")); // DateTimeAdd5HoursWithLeftMinPrecisionSecond
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10) + 5 hours = DateTime(2005, 5, 10)")); // DateTimeAdd5HoursWithLeftMinPrecisionDay
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10) + 25 hours = DateTime(2005, 5, 11)")); // DateTimeAdd5HoursWithLeftMinPrecisionDayOverflow
        Assert.True(await Helpers.CheckBool("Date(2014) + 24 months = @2016")); // DateAdd2YearsAsMonths
        Assert.True(await Helpers.CheckBool("Date(2014) + 25 months = @2016")); // DateAdd2YearsAsMonthsRem1
        Assert.True(await Helpers.CheckBool("Date(2014,6) + 33 days = @2014-07")); // DateAdd33Days
        Assert.True(await Helpers.CheckBool("Date(2014,6) + 1 year = @2015-06")); // DateAdd1Year
        Assert.True(await Helpers.CheckBool("DateTime(2016, 6, 10, 5) + 19 hours = @2016-06-11T00")); // DateTimeAddHoursOverflow
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10, 5, 5) + 5 minutes = @2005-05-10T05:10")); // DateTimeAdd5Minutes
        Assert.True(await Helpers.CheckBool("DateTime(2016, 6, 10, 5, 5) + 55 minutes = @2016-06-10T06:00")); // DateTimeAddMinutesOverflow
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10, 5, 5, 5) + 5 seconds = @2005-05-10T05:05:10")); // DateTimeAdd5Seconds
        Assert.True(await Helpers.CheckBool("DateTime(2016, 6, 10, 5, 5, 5) + 55 seconds = @2016-06-10T05:06:00")); // DateTimeAddSecondsOverflow
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10, 5, 5, 5, 5) + 5 milliseconds = @2005-05-10T05:05:05.010")); // DateTimeAdd5Milliseconds
        Assert.True(await Helpers.CheckBool("DateTime(2016, 6, 10, 5, 5, 5, 5) + 995 milliseconds = @2016-06-10T05:05:06.000")); // DateTimeAddMillisecondsOverflow
        Assert.True(await Helpers.CheckBool("DateTime(2012, 2, 29) + 1 year = @2013-02-28T")); // DateTimeAddLeapYear
        Assert.True(await Helpers.CheckBool("DateTime(2014) + 24 months = @2016T")); // DateTimeAdd2YearsByMonths
        Assert.True(await Helpers.CheckBool("DateTime(2014) + 730 days = @2016T")); // DateTimeAdd2YearsByDays
        Assert.True(await Helpers.CheckBool("DateTime(2014) + 735 days = @2016T")); // DateTimeAdd2YearsByDaysRem5Days
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 + 5 hours = @T20:59:59.999")); // TimeAdd5Hours
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 + 1 minute = @T16:00:59.999")); // TimeAdd1Minute
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 + 1 seconds = @T16:00:00.999")); // TimeAdd1Second
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 + 1 milliseconds = @T16:00:00.000")); // TimeAdd1Millisecond
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 + 5 hours + 1 minutes = @T21:00:59.999")); // TimeAdd5Hours1Minute
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 + 300 minutes = @T20:59:59.999")); // TimeAdd5hoursByMinute
    }

    [Fact]
    public async Task After()
    {
        Assert.True(await Helpers.CheckBool("DateTime(2005, 10, 10) after year of DateTime(2004, 10, 10)")); // DateTimeAfterYearTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 11, 10) after year of DateTime(2004, 10, 10)")); // DateTimeAfterYearFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 12, 10) after month of DateTime(2004, 11, 10)")); // DateTimeAfterMonthTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 9, 10) after month of DateTime(2004, 10, 10)")); // DateTimeAfterMonthFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 12, 11) after day of DateTime(2004, 10, 10)")); // DateTimeAfterDayTrue
        Assert.True(await Helpers.CheckBool("DateTime(2004, 12, 09) after day of DateTime(2003, 10, 10)")); // DateTimeAfterDayTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2004, 10, 9) after day of DateTime(2004, 10, 10)")); // DateTimeAfterDayFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 10, 10, 10) after hour of DateTime(2004, 10, 10, 5)")); // DateTimeAfterHourTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 10, 10, 20) after hour of DateTime(2004, 10, 10, 21)")); // DateTimeAfterHourFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 30) after minute of DateTime(2004, 10, 10, 20, 29)")); // DateTimeAfterMinuteTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 30) after minute of DateTime(2004, 10, 10, 20, 31)")); // DateTimeAfterMinuteFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 30, 15) after second of DateTime(2004, 10, 10, 20, 30, 14)")); // DateTimeAfterSecondTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 30, 15) after second of DateTime(2004, 10, 10, 20, 30, 16)")); // DateTimeAfterSecondFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 30, 15, 512) after millisecond of DateTime(2004, 10, 10, 20, 30, 15, 510)")); // DateTimeAfterMillisecondTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 30, 15, 512) after millisecond of DateTime(2004, 10, 10, 20, 30, 15, 513)")); // DateTimeAfterMillisecondFalse
        Assert.True(await Helpers.CheckBool("DateTime(2005, 10, 10) after day of DateTime(2005, 9)")); // DateTimeAfterUncertain
        Assert.True(await Helpers.CheckBool("@2012-03-10T10:20:00.999+07:00 after hour of @2012-03-10T08:20:00.999+06:00")); // AfterTimezoneTrue
        Assert.False(await Helpers.CheckBool("@2012-03-10T10:20:00.999+07:00 after hour of @2012-03-10T10:20:00.999+06:00")); // AfterTimezoneFalse
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 after hour of @T14:59:59.999")); // TimeAfterHourTrue
        Assert.False(await Helpers.CheckBool("@T15:59:59.999 after hour of @T16:59:59.999")); // TimeAfterHourFalse
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 after minute of @T15:58:59.999")); // TimeAfterMinuteTrue
        Assert.False(await Helpers.CheckBool("@T15:58:59.999 after minute of @T15:59:59.999")); // TimeAfterMinuteFalse
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 after second of @T15:59:58.999")); // TimeAfterSecondTrue
        Assert.False(await Helpers.CheckBool("@T15:59:58.999 after second of @T15:59:59.999")); // TimeAfterSecondFalse
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 after millisecond of @T15:59:59.998")); // TimeAfterMillisecondTrue
        Assert.False(await Helpers.CheckBool("@T15:59:59.998 after millisecond of @T15:59:59.999")); // TimeAfterMillisecondFalse
        Assert.True(await Helpers.CheckBool("Time(12, 30) after hour of Time(11, 55)")); // TimeAfterTimeCstor
    }

    [Fact]
    public async Task Before()
    {
        Assert.True(await Helpers.CheckBool("DateTime(2003) before year of DateTime(2004, 10, 10)")); // DateTimeBeforeYearTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 11, 10) before year of DateTime(2003, 10, 10)")); // DateTimeBeforeYearFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 10, 10) before month of DateTime(2004, 11, 10)")); // DateTimeBeforeMonthTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 11, 10) before month of DateTime(2004, 10, 10)")); // DateTimeBeforeMonthFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 10, 1) before day of DateTime(2004, 10, 10)")); // DateTimeBeforeDayTrue
        Assert.True(await Helpers.CheckBool("DateTime(2003, 10, 11) before day of DateTime(2004, 10, 10)")); // DateTimeBeforeDayTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2004, 10, 11) before day of DateTime(2004, 10, 10)")); // DateTimeBeforeDayFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 10, 10, 1) before hour of DateTime(2004, 10, 10, 5)")); // DateTimeBeforeHourTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 10, 10, 23) before hour of DateTime(2004, 10, 10, 21)")); // DateTimeBeforeHourFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 28) before minute of DateTime(2004, 10, 10, 20, 29)")); // DateTimeBeforeMinuteTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 35) before minute of DateTime(2004, 10, 10, 20, 31)")); // DateTimeBeforeMinuteFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 30, 12) before second of DateTime(2004, 10, 10, 20, 30, 14)")); // DateTimeBeforeSecondTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 30, 55) before second of DateTime(2004, 10, 10, 20, 30, 16)")); // DateTimeBeforeSecondFalse
        Assert.True(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 30, 15, 508) before millisecond of DateTime(2004, 10, 10, 20, 30, 15, 510)")); // DateTimeBeforeMillisecondTrue
        Assert.False(await Helpers.CheckBool("DateTime(2004, 10, 10, 20, 30, 15, 599) before millisecond of DateTime(2004, 10, 10, 20, 30, 15, 513)")); // DateTimeBeforeMillisecondFalse
        Assert.True(await Helpers.CheckBool("@2012-03-10T10:20:00.999+07:00 before hour of @2012-03-10T10:20:00.999+06:00")); // BeforeTimezoneTrue
        Assert.False(await Helpers.CheckBool("@2012-03-10T10:20:00.999+07:00 before hour of @2012-03-10T09:20:00.999+06:00")); // BeforeTimezoneFalse
        Assert.True(await Helpers.CheckBool("@T13:59:59.999 before hour of @T14:59:59.999")); // TimeBeforeHourTrue
        Assert.False(await Helpers.CheckBool("@T16:59:59.999 before hour of @T15:59:59.999")); // TimeBeforeHourFalse
        Assert.True(await Helpers.CheckBool("@T15:57:59.999 before minute of @T15:58:59.999")); // TimeBeforeMinuteTrue
        Assert.False(await Helpers.CheckBool("@T15:59:59.999 before minute of @T15:59:59.999")); // TimeBeforeMinuteFalse
        Assert.True(await Helpers.CheckBool("@T15:59:57.999 before second of @T15:59:58.999")); // TimeBeforeSecondTrue
        Assert.False(await Helpers.CheckBool("@T15:59:56.999 before second of @T15:59:55.999")); // TimeBeforeSecondFalse
        Assert.True(await Helpers.CheckBool("@T15:59:59.997 before millisecond of @T15:59:59.998")); // TimeBeforeMillisecondTrue
        Assert.False(await Helpers.CheckBool("@T15:59:59.998 before millisecond of @T15:59:59.997")); // TimeBeforeMillisecondFalse
    }

    [Fact]
    public async Task DateTime()
    {
        Assert.True(await Helpers.CheckBool("DateTime(2003) = @2003T")); // DateTimeYear
        Assert.True(await Helpers.CheckBool("DateTime(2003, 10) = @2003-10T")); // DateTimeMonth
        Assert.True(await Helpers.CheckBool("DateTime(2003, 10, 29) = @2003-10-29T")); // DateTimeDay
        Assert.True(await Helpers.CheckBool("DateTime(2003, 10, 29, 20) = @2003-10-29T20")); // DateTimeHour
        Assert.True(await Helpers.CheckBool("DateTime(2003, 10, 29, 20, 50) = @2003-10-29T20:50")); // DateTimeMinute
        Assert.True(await Helpers.CheckBool("DateTime(2003, 10, 29, 20, 50, 33) = @2003-10-29T20:50:33")); // DateTimeSecond
        Assert.True(await Helpers.CheckBool("DateTime(2003, 10, 29, 20, 50, 33, 955) = @2003-10-29T20:50:33.955")); // DateTimeMillisecond
    }

    [Fact]
    public async Task DateTimeComponentFrom()
    {
        Assert.True(await Helpers.CheckBool("year from DateTime(2003, 10, 29, 20, 50, 33, 955) = 2003")); // DateTimeComponentFromYear
        Assert.True(await Helpers.CheckBool("month from DateTime(2003, 10, 29, 20, 50, 33, 955) = 10")); // DateTimeComponentFromMonth
        Assert.True(await Helpers.CheckBool("month from DateTime(2003, 01, 29, 20, 50, 33, 955) = 1")); // DateTimeComponentFromMonthMinBoundary
        Assert.True(await Helpers.CheckBool("day from DateTime(2003, 10, 29, 20, 50, 33, 955) = 29")); // DateTimeComponentFromDay
        Assert.True(await Helpers.CheckBool("hour from DateTime(2003, 10, 29, 20, 50, 33, 955) = 20")); // DateTimeComponentFromHour
        Assert.True(await Helpers.CheckBool("minute from DateTime(2003, 10, 29, 20, 50, 33, 955) = 50")); // DateTimeComponentFromMinute
        Assert.True(await Helpers.CheckBool("second from DateTime(2003, 10, 29, 20, 50, 33, 955) = 33")); // DateTimeComponentFromSecond
        Assert.True(await Helpers.CheckBool("millisecond from DateTime(2003, 10, 29, 20, 50, 33, 955) = 955")); // DateTimeComponentFromMillisecond
        Assert.True(await Helpers.CheckBool("timezone from DateTime(2003, 10, 29, 20, 50, 33, 955, 1) = ")); // DateTimeComponentFromTimezone
        Assert.True(await Helpers.CheckBool("timezoneoffset from DateTime(2003, 10, 29, 20, 50, 33, 955, 1) = 1.00")); // DateTimeComponentFromTimezone2
        Assert.True(await Helpers.CheckBool("date from DateTime(2003, 10, 29, 20, 50, 33, 955, 1) = @2003-10-29")); // DateTimeComponentFromDate
        Assert.True(await Helpers.CheckBool("hour from @T23:20:15.555 = 23")); // TimeComponentFromHour
        Assert.True(await Helpers.CheckBool("minute from @T23:20:15.555 = 20")); // TimeComponentFromMinute
        Assert.True(await Helpers.CheckBool("second from @T23:20:15.555 = 15")); // TimeComponentFromSecond
        Assert.True(await Helpers.CheckBool("millisecond from @T23:20:15.555 = 555")); // TimeComponentFromMilli
    }

    [Fact]
    public async Task Difference()
    {
        Assert.True(await Helpers.CheckBool("difference in years between DateTime(2000) and DateTime(2005, 12) = 5")); // DateTimeDifferenceYear
        Assert.True(await Helpers.CheckBool("difference in months between DateTime(2000, 2) and DateTime(2000, 10) = 8")); // DateTimeDifferenceMonth
        Assert.True(await Helpers.CheckBool("difference in days between DateTime(2000, 10, 15, 10, 30) and DateTime(2000, 10, 25, 10, 0) = 10")); // DateTimeDifferenceDay
        Assert.True(await Helpers.CheckBool("difference in hours between DateTime(2000, 4, 1, 12) and DateTime(2000, 4, 1, 20) = 8")); // DateTimeDifferenceHour
        Assert.True(await Helpers.CheckBool("difference in minutes between DateTime(2005, 12, 10, 5, 16) and DateTime(2005, 12, 10, 5, 25) = 9")); // DateTimeDifferenceMinute
        Assert.True(await Helpers.CheckBool("difference in seconds between DateTime(2000, 10, 10, 10, 5, 45) and DateTime(2000, 10, 10, 10, 5, 50) = 5")); // DateTimeDifferenceSecond
        Assert.True(await Helpers.CheckBool("difference in milliseconds between DateTime(2000, 10, 10, 10, 5, 45, 500, -6.0) and DateTime(2000, 10, 10, 10, 5, 45, 900, -7.0) = 3600400")); // DateTimeDifferenceMillisecond
        Assert.True(await Helpers.CheckBool("difference in weeks between DateTime(2000, 10, 15) and DateTime(2000, 10, 28) = 1")); // DateTimeDifferenceWeeks
        Assert.True(await Helpers.CheckBool("difference in weeks between DateTime(2000, 10, 15) and DateTime(2000, 10, 29) = 2")); // DateTimeDifferenceWeeks2
        Assert.True(await Helpers.CheckBool("difference in weeks between @2012-03-10T22:05:09 and @2012-03-24T07:19:33 = 2")); // DateTimeDifferenceWeeks3
        Assert.True(await Helpers.CheckBool("difference in years between DateTime(2016) and DateTime(1998) = -18")); // DateTimeDifferenceNegative
        Assert.True(await Helpers.CheckBool("difference in months between DateTime(2005) and DateTime(2006, 7) > 5")); // DateTimeDifferenceUncertain
        Assert.True(await Helpers.CheckBool("difference in hours between @T20 and @T23:25:15.555 = 3")); // TimeDifferenceHour
        Assert.True(await Helpers.CheckBool("difference in minutes between @T20:20:15.555 and @T20:25:15.555 = 5")); // TimeDifferenceMinute
        Assert.True(await Helpers.CheckBool("difference in seconds between @T20:20:15.555 and @T20:20:20.555 = 5")); // TimeDifferenceSecond
        Assert.True(await Helpers.CheckBool("difference in milliseconds between @T20:20:15.555 and @T20:20:15.550 = -5")); // TimeDifferenceMillis
    }

    [Fact]
    public async Task FromGithubissue29()
    {
        Assert.True(await Helpers.CheckBool("@2017-03-12T01:00:00-07:00 = @2017-03-12T01:00:00-07:00")); // DateTimeA
        Assert.True(await Helpers.CheckBool("DateTime(2017, 3, 12, 1, 0, 0, 0, -7.0) = @2017-03-12T01:00:00.000-07:00")); // DateTimeAA
        Assert.True(await Helpers.CheckBool("@2017-03-12T03:00:00-06:00 = @2017-03-12T03:00:00-06:00")); // DateTimeB
        Assert.True(await Helpers.CheckBool("DateTime(2017, 3, 12, 3, 0, 0, 0, -6.0) = @2017-03-12T03:00:00.000-06:00")); // DateTimeBB
        Assert.True(await Helpers.CheckBool("@2017-11-05T01:30:00-06:00 = @2017-11-05T01:30:00-06:00")); // DateTimeC
        Assert.True(await Helpers.CheckBool("DateTime(2017, 11, 5, 1, 30, 0, 0, -6.0) = @2017-11-05T01:30:00.000-06:00")); // DateTimeCC
        Assert.True(await Helpers.CheckBool("@2017-11-05T01:15:00-07:00 = @2017-11-05T01:15:00-07:00")); // DateTimeD
        Assert.True(await Helpers.CheckBool("DateTime(2017, 11, 5, 1, 15, 0, 0, -7.0) = @2017-11-05T01:15:00.000-07:00")); // DateTimeDD
        Assert.True(await Helpers.CheckBool("@2017-03-12T00:00:00-07:00 = @2017-03-12T00:00:00-07:00")); // DateTimeE
        Assert.True(await Helpers.CheckBool("DateTime(2017, 3, 12, 0, 0, 0, 0, -7.0) = @2017-03-12T00:00:00.000-07:00")); // DateTimeEE
        Assert.True(await Helpers.CheckBool("@2017-03-13T00:00:00-06:00 = @2017-03-13T00:00:00-06:00")); // DateTimeF
        Assert.True(await Helpers.CheckBool("DateTime(2017, 3, 13, 0, 0, 0, 0, -6.0) = @2017-03-13T00:00:00.000-06:00")); // DateTimeFF
        Assert.True(await Helpers.CheckBool("difference in hours between @2017-03-12T01:00:00-07:00 and @2017-03-12T03:00:00-06:00 = 1")); // DifferenceInHoursA
        Assert.True(await Helpers.CheckBool("difference in minutes between @2017-11-05T01:30:00-06:00 and @2017-11-05T01:15:00-07:00 = 45")); // DifferenceInMinutesA
        Assert.True(await Helpers.CheckBool("difference in days between @2017-03-12T00:00:00-07:00 and @2017-03-13T00:00:00-06:00 = 1")); // DifferenceInDaysA
        Assert.True(await Helpers.CheckBool("difference in hours between DateTime(2017, 3, 12, 1, 0, 0, 0, -7.0) and DateTime(2017, 3, 12, 3, 0, 0, 0, -6.0) = 1")); // DifferenceInHoursAA
        Assert.True(await Helpers.CheckBool("difference in minutes between DateTime(2017, 11, 5, 1, 30, 0, 0, -6.0) and DateTime(2017, 11, 5, 1, 15, 0, 0, -7.0) = 45")); // DifferenceInMinutesAA
        Assert.True(await Helpers.CheckBool("difference in days between DateTime(2017, 3, 12, 0, 0, 0, 0, -7.0) and DateTime(2017, 3, 13, 0, 0, 0, 0, -6.0) = 1")); // DifferenceInDaysAA
    }

    [Fact]
    public async Task Duration()
    {
        Assert.True(await Helpers.CheckBool("years between DateTime(2005) and DateTime(2010) = Interval[ 4, 5 ]")); // DateTimeDurationBetweenYear
        Assert.True(await Helpers.CheckBool("years between DateTime(2005, 5) and DateTime(2010, 4) = 4")); // DateTimeDurationBetweenYearOffset
        Assert.True(await Helpers.CheckBool("months between @2014-01-31 and @2014-02-01 = 0")); // DateTimeDurationBetweenMonth
        Assert.True(await Helpers.CheckBool("days between DateTime(2010, 10, 12, 12, 5) and DateTime(2008, 8, 15, 8, 8) = -788")); // DateTimeDurationBetweenDaysDiffYears
    }

    [Fact]
    public async Task Uncertaintytests()
    {
        Assert.True(await Helpers.CheckBool("days between DateTime(2014, 1, 15) and DateTime(2014, 2) = Interval[ 16, 44 ]")); // DateTimeDurationBetweenUncertainInterval
        Assert.True(await Helpers.CheckBool("months between DateTime(2005) and DateTime(2006, 5) = Interval[ 4, 16 ]")); // DateTimeDurationBetweenUncertainInterval2
        Assert.True(await Helpers.CheckBool("(days between DateTime(2014, 1, 15) and DateTime(2014, 2))  + (days between DateTime(2014, 1, 15) and DateTime(2014, 2)) = Interval[ 32, 88 ]")); // DateTimeDurationBetweenUncertainAdd
        Assert.True(await Helpers.CheckBool("(days between DateTime(2014, 1, 15) and DateTime(2014, 2))  - (months between DateTime(2005) and DateTime(2006, 5)) = Interval[ 0, 40 ]")); // DateTimeDurationBetweenUncertainSubtract
        Assert.True(await Helpers.CheckBool("(days between DateTime(2014, 1, 15) and DateTime(2014, 2))  * (days between DateTime(2014, 1, 15) and DateTime(2014, 2)) = Interval[ 256, 1936 ]")); // DateTimeDurationBetweenUncertainMultiply
        Assert.True(await Helpers.CheckBool("(days between DateTime(2014, 1, 15) and DateTime(2014, 2))  div (months between DateTime(2005) and DateTime(2006, 5)) = ")); // DateTimeDurationBetweenUncertainDiv
        Assert.True(await Helpers.CheckBool("months between DateTime(2005) and DateTime(2006, 7) > 5")); // DateTimeDurationBetweenMonthUncertain
        Assert.True(await Helpers.CheckBool("(months between DateTime(2005) and DateTime(2006, 2) > 5) is null")); // DateTimeDurationBetweenMonthUncertain2
        Assert.False(await Helpers.CheckBool("months between DateTime(2005) and DateTime(2006, 7) > 25")); // DateTimeDurationBetweenMonthUncertain3
        Assert.True(await Helpers.CheckBool("months between DateTime(2005) and DateTime(2006, 7) < 24")); // DateTimeDurationBetweenMonthUncertain4
        Assert.False(await Helpers.CheckBool("months between DateTime(2005) and DateTime(2006, 7) = 24")); // DateTimeDurationBetweenMonthUncertain5
        Assert.True(await Helpers.CheckBool("months between DateTime(2005) and DateTime(2006, 7) >= 5")); // DateTimeDurationBetweenMonthUncertain6
        Assert.True(await Helpers.CheckBool("months between DateTime(2005) and DateTime(2006, 7) <= 24")); // DateTimeDurationBetweenMonthUncertain7
        Assert.True(await Helpers.CheckBool("@2012-03-10T10:20:00 = @2012-03-10T10:20:00")); // DateTime1
        Assert.True(await Helpers.CheckBool("@2013-03-10T09:20:00 = @2013-03-10T09:20:00")); // DateTime2
        Assert.True(await Helpers.CheckBool("years between (date from @2012-03-10T10:20:00) and (date from @2013-03-10T09:20:00) = 1")); // DurationInYears
        Assert.True(await Helpers.CheckBool("weeks between @2012-03-10T22:05:09 and @2012-03-20T07:19:33 = 1")); // DurationInWeeks
        Assert.True(await Helpers.CheckBool("weeks between @2012-03-10T22:05:09 and @2012-03-24T07:19:33 = 1")); // DurationInWeeks2
        Assert.True(await Helpers.CheckBool("weeks between @2012-03-10T06:05:09 and @2012-03-24T07:19:33 = 2")); // DurationInWeeks3
        Assert.True(await Helpers.CheckBool("hours between @T20:26:15.555 and @T23:25:15.555 = 2")); // TimeDurationBetweenHour
        Assert.True(await Helpers.CheckBool("hours between @T06Z and @T07:00:00Z = ")); // TimeDurationBetweenHourDiffPrecision
        Assert.True(await Helpers.CheckBool("hours between @T06 and @T07:00:00 = 1")); // TimeDurationBetweenHourDiffPrecision2
        Assert.True(await Helpers.CheckBool("minutes between @T23:20:16.555 and @T23:25:15.555 = 4")); // TimeDurationBetweenMinute
        Assert.True(await Helpers.CheckBool("seconds between @T23:25:10.556 and @T23:25:15.555 = 4")); // TimeDurationBetweenSecond
        Assert.True(await Helpers.CheckBool("milliseconds between @T23:25:25.555 and @T23:25:25.560 = 5")); // TimeDurationBetweenMillis
        Assert.True(await Helpers.CheckBool("hours between @2017-03-12T01:00:00-07:00 and @2017-03-12T03:00:00-06:00 = 1")); // DurationInHoursA
        Assert.True(await Helpers.CheckBool("minutes between @2017-11-05T01:30:00-06:00 and @2017-11-05T01:15:00-07:00 = 45")); // DurationInMinutesA
        Assert.True(await Helpers.CheckBool("days between @2017-03-12T00:00:00-07:00 and @2017-03-13T00:00:00-06:00 = 0")); // DurationInDaysA
        Assert.True(await Helpers.CheckBool("hours between DateTime(2017, 3, 12, 1, 0, 0, 0, -7.0) and DateTime(2017, 3, 12, 3, 0, 0, 0, -6.0) = 1")); // DurationInHoursAA
        Assert.True(await Helpers.CheckBool("minutes between DateTime(2017, 11, 5, 1, 30, 0, 0, -6.0) and DateTime(2017, 11, 5, 1, 15, 0, 0, -7.0) = 45")); // DurationInMinutesAA
        Assert.True(await Helpers.CheckBool("days between DateTime(2017, 3, 12, 0, 0, 0, 0, -7.0) and DateTime(2017, 3, 13, 0, 0, 0, 0, -6.0) = 0")); // DurationInDaysAA
    }

    [Fact]
    public async Task Now()
    {
        Assert.True(await Helpers.CheckBool("Now() = Now()")); // DateTimeNow
    }

    [Fact]
    public async Task SameAs()
    {
        Assert.True(await Helpers.CheckBool("DateTime(2014) same year as DateTime(2014)")); // DateTimeSameAsYearTrue
        Assert.False(await Helpers.CheckBool("DateTime(2013) same year as DateTime(2014)")); // DateTimeSameAsYearFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12) same month as DateTime(2014, 12)")); // DateTimeSameAsMonthTrue
        Assert.False(await Helpers.CheckBool("DateTime(2014, 12) same month as DateTime(2014, 10)")); // DateTimeSameAsMonthFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 10) same day as DateTime(2014, 12, 10)")); // DateTimeSameAsDayTrue
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 10) same day as DateTime(2014, 10, 11)")); // DateTimeSameAsDayFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 10, 20) same hour as DateTime(2014, 12, 10, 20)")); // DateTimeSameAsHourTrue
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 10, 20) same hour as DateTime(2014, 10, 10, 21)")); // DateTimeSameAsHourFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 10, 20, 55) same minute as DateTime(2014, 12, 10, 20, 55)")); // DateTimeSameAsMinuteTrue
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 10, 20, 55) same minute as DateTime(2014, 10, 10, 21, 56)")); // DateTimeSameAsMinuteFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 10, 20, 55, 45) same second as DateTime(2014, 12, 10, 20, 55, 45)")); // DateTimeSameAsSecondTrue
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 10, 20, 55, 45) same second as DateTime(2014, 10, 10, 21, 55, 44)")); // DateTimeSameAsSecondFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 10, 20, 55, 45, 500) same millisecond as DateTime(2014, 12, 10, 20, 55, 45, 500)")); // DateTimeSameAsMillisecondTrue
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 10, 20, 55, 45, 500) same millisecond as DateTime(2014, 10, 10, 21, 55, 45, 501)")); // DateTimeSameAsMillisecondFalse
        Assert.True(await Helpers.CheckBool("(DateTime(2014, 10) same day as DateTime(2014, 10, 12)) is null")); // DateTimeSameAsNull
        Assert.True(await Helpers.CheckBool("@2012-03-10T10:20:00.999+07:00 same hour as @2012-03-10T09:20:00.999+06:00")); // SameAsTimezoneTrue
        Assert.False(await Helpers.CheckBool("@2012-03-10T10:20:00.999+07:00 same hour as @2012-03-10T10:20:00.999+06:00")); // SameAsTimezoneFalse
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same hour as @T23:55:25.900")); // TimeSameAsHourTrue
        Assert.False(await Helpers.CheckBool("@T22:25:25.555 same hour as @T23:25:25.555")); // TimeSameAsHourFalse
        Assert.True(await Helpers.CheckBool("@T23:55:22.555 same minute as @T23:55:25.900")); // TimeSameAsMinuteTrue
        Assert.False(await Helpers.CheckBool("@T23:26:25.555 same minute as @T23:25:25.555")); // TimeSameAsMinuteFalse
        Assert.True(await Helpers.CheckBool("@T23:55:25.555 same second as @T23:55:25.900")); // TimeSameAsSecondTrue
        Assert.False(await Helpers.CheckBool("@T23:25:35.555 same second as @T23:25:25.555")); // TimeSameAsSecondFalse
        Assert.True(await Helpers.CheckBool("@T23:55:25.555 same millisecond as @T23:55:25.555")); // TimeSameAsMillisTrue
        Assert.False(await Helpers.CheckBool("@T23:25:25.555 same millisecond as @T23:25:25.554")); // TimeSameAsMillisFalse
    }

    [Fact]
    public async Task SameOrAfter()
    {
        Assert.True(await Helpers.CheckBool("DateTime(2014) same year or after DateTime(2014)")); // DateTimeSameOrAfterYearTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2016) same year or after DateTime(2014)")); // DateTimeSameOrAfterYearTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2013) same year or after DateTime(2014)")); // DateTimeSameOrAfterYearFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12) same month or after DateTime(2014, 12)")); // DateTimeSameOrAfterMonthTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 10) same month or after DateTime(2014, 9)")); // DateTimeSameOrAfterMonthTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10) same month or after DateTime(2014, 11)")); // DateTimeSameOrAfterMonthFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 20) same day or after DateTime(2014, 12, 20)")); // DateTimeSameOrAfterDayTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 10, 25) same day or after DateTime(2014, 10, 20)")); // DateTimeSameOrAfterDayTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 20) same day or after DateTime(2014, 10, 25)")); // DateTimeSameOrAfterDayFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 20, 12) same hour or after DateTime(2014, 12, 20, 12)")); // DateTimeSameOrAfterHourTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 10, 25, 12) same hour or after DateTime(2014, 10, 25, 10)")); // DateTimeSameOrAfterHourTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 25, 12) same hour or after DateTime(2014, 10, 25, 15)")); // DateTimeSameOrAfterHourFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 20, 12, 30) same minute or after DateTime(2014, 12, 20, 12, 30)")); // DateTimeSameOrAfterMinuteTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 10, 25, 10, 30) same minute or after DateTime(2014, 10, 25, 10, 25)")); // DateTimeSameOrAfterMinuteTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 25, 15, 30) same minute or after DateTime(2014, 10, 25, 15, 45)")); // DateTimeSameOrAfterMinuteFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 20, 12, 30, 15) same second or after DateTime(2014, 12, 20, 12, 30, 15)")); // DateTimeSameOrAfterSecondTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 10, 25, 10, 25, 25) same second or after DateTime(2014, 10, 25, 10, 25, 20)")); // DateTimeSameOrAfterSecondTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 25, 15, 45, 20) same second or after DateTime(2014, 10, 25, 15, 45, 21)")); // DateTimeSameOrAfterSecondFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 20, 12, 30, 15, 250) same millisecond or after DateTime(2014, 12, 20, 12, 30, 15, 250)")); // DateTimeSameOrAfterMillisecondTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 10, 25, 10, 25, 20, 500) same millisecond or after DateTime(2014, 10, 25, 10, 25, 20, 499)")); // DateTimeSameOrAfterMillisecondTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 25, 15, 45, 20, 500) same millisecond or after DateTime(2014, 10, 25, 15, 45, 20, 501)")); // DateTimeSameOrAfterMillisecondFalse
        Assert.True(await Helpers.CheckBool("(DateTime(2014, 12, 20) same day or after DateTime(2014, 12)) is null")); // DateTimeSameOrAfterNull1
        Assert.True(await Helpers.CheckBool("@2012-03-10T10:20:00.999+07:00 same hour or after @2012-03-10T09:20:00.999+06:00")); // SameOrAfterTimezoneTrue
        Assert.False(await Helpers.CheckBool("@2012-03-10T10:20:00.999+07:00 same hour or after @2012-03-10T10:20:00.999+06:00")); // SameOrAfterTimezoneFalse
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same hour or after @T23:55:25.900")); // TimeSameOrAfterHourTrue1
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same hour or after @T22:55:25.900")); // TimeSameOrAfterHourTrue2
        Assert.False(await Helpers.CheckBool("@T22:25:25.555 same hour or after @T23:55:25.900")); // TimeSameOrAfterHourFalse
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same minute or after @T23:25:25.900")); // TimeSameOrAfterMinuteTrue1
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same minute or after @T22:15:25.900")); // TimeSameOrAfterMinuteTrue2
        Assert.False(await Helpers.CheckBool("@T23:25:25.555 same minute or after @T23:55:25.900")); // TimeSameOrAfterMinuteFalse
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same second or after @T23:25:25.900")); // TimeSameOrAfterSecondTrue1
        Assert.True(await Helpers.CheckBool("@T23:25:35.555 same second or after @T22:25:25.900")); // TimeSameOrAfterSecondTrue2
        Assert.False(await Helpers.CheckBool("@T23:55:25.555 same second or after @T23:55:35.900")); // TimeSameOrAfterSecondFalse
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same millisecond or after @T23:25:25.555")); // TimeSameOrAfterMillisTrue1
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same millisecond or after @T22:25:25.550")); // TimeSameOrAfterMillisTrue2
        Assert.False(await Helpers.CheckBool("@T23:55:25.555 same millisecond or after @T23:55:25.900")); // TimeSameOrAfterMillisFalse
        Assert.True(await Helpers.CheckBool("@2017-12-20T11:00:00.000 on or after @2017-12-20T11:00:00.000")); // OnOrAfterTrue
        Assert.True(await Helpers.CheckBool("@2017-12-21T02:00:00.0 same or after @2017-12-20T11:00:00.0")); // Issue32DateTime
    }

    [Fact]
    public async Task SameOrBefore()
    {
        Assert.True(await Helpers.CheckBool("DateTime(2014) same year or before DateTime(2014)")); // DateTimeSameOrBeforeYearTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2013) same year or before DateTime(2014)")); // DateTimeSameOrBeforeYearTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2015) same year or before DateTime(2014)")); // DateTimeSameOrBeforeYearFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12) same month or before DateTime(2014, 12)")); // DateTimeSameOrBeforeMonthTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 8) same month or before DateTime(2014, 9)")); // DateTimeSameOrBeforeMonthTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 12) same month or before DateTime(2014, 11)")); // DateTimeSameOrBeforeMonthFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 20) same day or before DateTime(2014, 12, 20)")); // DateTimeSameOrBeforeDayTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 10, 15) same day or before DateTime(2014, 10, 20)")); // DateTimeSameOrBeforeDayTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 30) same day or before DateTime(2014, 10, 25)")); // DateTimeSameOrBeforeDayFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 20, 12) same hour or before DateTime(2014, 12, 20, 12)")); // DateTimeSameOrBeforeHourTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 10, 25, 5) same hour or before DateTime(2014, 10, 25, 10)")); // DateTimeSameOrBeforeHourTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 25, 20) same hour or before DateTime(2014, 10, 25, 15)")); // DateTimeSameOrBeforeHourFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 20, 12, 30) same minute or before DateTime(2014, 12, 20, 12, 30)")); // DateTimeSameOrBeforeMinuteTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 10, 25, 10, 20) same minute or before DateTime(2014, 10, 25, 10, 25)")); // DateTimeSameOrBeforeMinuteTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 25, 15, 55) same minute or before DateTime(2014, 10, 25, 15, 45)")); // DateTimeSameOrBeforeMinuteFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 20, 12, 30, 15) same second or before DateTime(2014, 12, 20, 12, 30, 15)")); // DateTimeSameOrBeforeSecondTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 10, 25, 10, 25, 15) same second or before DateTime(2014, 10, 25, 10, 25, 20)")); // DateTimeSameOrBeforeSecondTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 25, 15, 45, 25) same second or before DateTime(2014, 10, 25, 15, 45, 21)")); // DateTimeSameOrBeforeSecondFalse
        Assert.True(await Helpers.CheckBool("DateTime(2014, 12, 20, 12, 30, 15, 250) same millisecond or before DateTime(2014, 12, 20, 12, 30, 15, 250)")); // DateTimeSameOrBeforeMillisecondTrue1
        Assert.True(await Helpers.CheckBool("DateTime(2014, 10, 25, 10, 25, 20, 450) same millisecond or before DateTime(2014, 10, 25, 10, 25, 20, 499)")); // DateTimeSameOrBeforeMillisecondTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2014, 10, 25, 15, 45, 20, 505) same millisecond or before DateTime(2014, 10, 25, 15, 45, 20, 501)")); // DateTimeSameOrBeforeMillisecondFalse
        Assert.True(await Helpers.CheckBool("(DateTime(2014, 12, 20) same minute or before DateTime(2014, 12, 20, 15)) is null")); // DateTimeSameOrBeforeNull1
        Assert.True(await Helpers.CheckBool("@2012-03-10T09:20:00.999+07:00 same hour or before @2012-03-10T10:20:00.999+06:00")); // SameOrBeforeTimezoneTrue
        Assert.False(await Helpers.CheckBool("@2012-03-10T10:20:00.999+06:00 same hour or before @2012-03-10T10:20:00.999+07:00")); // SameOrBeforeTimezoneFalse
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same hour or before @T23:55:25.900")); // TimeSameOrBeforeHourTrue1
        Assert.True(await Helpers.CheckBool("@T21:25:25.555 same hour or before @T22:55:25.900")); // TimeSameOrBeforeHourTrue2
        Assert.False(await Helpers.CheckBool("@T22:25:25.555 same hour or before @T21:55:25.900")); // TimeSameOrBeforeHourFalse
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same minute or before @T23:25:25.900")); // TimeSameOrBeforeMinuteTrue1
        Assert.False(await Helpers.CheckBool("@T23:10:25.555 same minute or before @T22:15:25.900")); // TimeSameOrBeforeMinuteFalse0
        Assert.False(await Helpers.CheckBool("@T23:56:25.555 same minute or before @T23:55:25.900")); // TimeSameOrBeforeMinuteFalse
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same second or before @T23:25:25.900")); // TimeSameOrBeforeSecondTrue1
        Assert.False(await Helpers.CheckBool("@T23:25:35.555 same second or before @T22:25:45.900")); // TimeSameOrBeforeSecondFalse0
        Assert.False(await Helpers.CheckBool("@T23:55:45.555 same second or before @T23:55:35.900")); // TimeSameOrBeforeSecondFalse
        Assert.True(await Helpers.CheckBool("@T23:25:25.555 same millisecond or before @T23:25:25.555")); // TimeSameOrBeforeMillisTrue1
        Assert.False(await Helpers.CheckBool("@T23:25:25.200 same millisecond or before @T22:25:25.550")); // TimeSameOrBeforeMillisFalse0
        Assert.False(await Helpers.CheckBool("@T23:55:25.966 same millisecond or before @T23:55:25.900")); // TimeSameOrBeforeMillisFalse
    }

    [Fact]
    public async Task Subtract()
    {
        Assert.True(await Helpers.CheckBool("DateTime(2005, 10, 10) - 5 years = @2000-10-10T")); // DateTimeSubtract5Years
        Assert.True(await Helpers.CheckBool("DateTime(2005, 10, 10) - 2005 years = ")); // DateTimeSubtractInvalidYears
        Assert.True(await Helpers.CheckBool("DateTime(2005, 6, 10) - 5 months = @2005-01-10T")); // DateTimeSubtract5Months
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10) - 6 months = @2004-11-10T")); // DateTimeSubtractMonthsUnderflow
        Assert.True(await Helpers.CheckBool("DateTime(2018, 5, 23) - 3 weeks = DateTime(2018, 5, 2)")); // DateTimeSubtractThreeWeeks
        Assert.True(await Helpers.CheckBool("DateTime(2018, 5, 23) - 52 weeks = DateTime(2017, 5, 24)")); // DateTimeSubtractYearInWeeks
        Assert.True(await Helpers.CheckBool("DateTime(2024, 2, 29) - 52 weeks = DateTime(2023, 3, 2)")); // DateTimeLeapDaySubtractYearInWeeks
        Assert.True(await Helpers.CheckBool("DateTime(2024, 3, 1) - 52 weeks = DateTime(2023, 3, 3)")); // DateTimeLeapYearSubtractYearInWeeks
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10) - 5 days = @2005-05-05T")); // DateTimeSubtract5Days
        Assert.True(await Helpers.CheckBool("DateTime(2016, 6, 10) - 11 days = @2016-05-30T")); // DateTimeSubtractDaysUnderflow
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10, 10) - 5 hours = @2005-05-10T05")); // DateTimeSubtract5Hours
        Assert.True(await Helpers.CheckBool("DateTime(2016, 6, 10, 5) - 6 hours = @2016-06-09T23")); // DateTimeSubtractHoursUnderflow
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10, 5, 10) - 5 minutes = @2005-05-10T05:05")); // DateTimeSubtract5Minutes
        Assert.True(await Helpers.CheckBool("DateTime(2016, 6, 10, 5, 5) - 6 minutes = @2016-06-10T04:59")); // DateTimeSubtractMinutesUnderflow
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10, 5, 5, 10) - 5 seconds = @2005-05-10T05:05:05")); // DateTimeSubtract5Seconds
        Assert.True(await Helpers.CheckBool("DateTime(2016,5) - 31535999 seconds = DateTime(2015, 5)")); // DateTimeSubtract1YearInSeconds
        Assert.True(await Helpers.CheckBool("DateTime(2016, 10, 1, 10, 20, 30) - 15 hours = @2016-09-30T19:20:30")); // DateTimeSubtract15HourPrecisionSecond
        Assert.True(await Helpers.CheckBool("DateTime(2016, 6, 10, 5, 5, 5) - 6 seconds = @2016-06-10T05:04:59")); // DateTimeSubtractSecondsUnderflow
        Assert.True(await Helpers.CheckBool("DateTime(2005, 5, 10, 5, 5, 5, 10) - 5 milliseconds = @2005-05-10T05:05:05.005")); // DateTimeSubtract5Milliseconds
        Assert.True(await Helpers.CheckBool("DateTime(2016, 6, 10, 5, 5, 5, 5) - 6 milliseconds = @2016-06-10T05:05:04.999")); // DateTimeSubtractMillisecondsUnderflow
        Assert.True(await Helpers.CheckBool("DateTime(2014) - 24 months = @2012T")); // DateTimeSubtract2YearsAsMonths
        Assert.True(await Helpers.CheckBool("DateTime(2014) - 25 months = @2012T")); // DateTimeSubtract2YearsAsMonthsRem1
        Assert.True(await Helpers.CheckBool("Date(2014) - 24 months = @2012")); // DateSubtract2YearsAsMonths
        Assert.True(await Helpers.CheckBool("Date(2014) - 25 months = @2012")); // DateSubtract2YearsAsMonthsRem1
        Assert.True(await Helpers.CheckBool("Date(2014,6) - 33 days = @2014-05")); // DateSubtract33Days
        Assert.True(await Helpers.CheckBool("Date(2014,6) - 1 year = @2013-06")); // DateSubtract1Year
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 - 5 hours = @T10:59:59.999")); // TimeSubtract5Hours
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 - 1 minutes = @T15:58:59.999")); // TimeSubtract1Minute
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 - 1 seconds = @T15:59:58.999")); // TimeSubtract1Second
        Assert.True(await Helpers.CheckBool("@T15:59:59.0 - 1 milliseconds = @T15:59:58.999")); // TimeSubtract1Millisecond
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 - 5 hours - 1 minutes = @T10:58:59.999")); // TimeSubtract5Hours1Minute
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 - 300 minutes = @T10:59:59.999")); // TimeSubtract5hoursByMinute
    }

    [Fact]
    public async Task Time()
    {
        Assert.True(await Helpers.CheckBool("@T23:59:59.999 = @T23:59:59.999")); // TimeTest2
    }

    [Fact]
    public async Task TimeOfDay()
    {
        Assert.True(await Helpers.CheckBool("TimeOfDay() = TimeOfDay()")); // TimeOfDayTest
    }

    [Fact]
    public async Task Today()
    {
        Assert.True(await Helpers.CheckBool("Today() same day or before Today()")); // DateTimeSameOrBeforeTodayTrue1
        Assert.True(await Helpers.CheckBool("Today() same day or before Today() + 1 days")); // DateTimeSameOrBeforeTodayTrue2
        Assert.False(await Helpers.CheckBool("Today() + 1 years same day or before Today()")); // DateTimeSameOrBeforeTodayFalse
        Assert.True(await Helpers.CheckBool("Today() + 1 days > Today()")); // DateTimeAddTodayTrue
        Assert.True(await Helpers.CheckBool("Today() = Today()")); // Issue34B
    }
}
