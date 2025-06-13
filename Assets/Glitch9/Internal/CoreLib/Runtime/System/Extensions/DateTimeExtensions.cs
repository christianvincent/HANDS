using System;
using System.Collections.Generic;
using System.Globalization;

namespace Glitch9
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime date, DayOfWeek startingDay /* sunday or monday */)
        {
            int diff = (7 + (date.DayOfWeek - startingDay)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        public static int GetWeekOfMonth(this DateTime date)
        {
            DateTime beginningOfMonth = new(date.Year, date.Month, 1);

            while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);

            return (int)Math.Truncate((double)date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;
        }

        public static IEnumerable<DateTime> GetDatesUntil(this DateTime startDate, DateTime endDate)
        {
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                yield return date;
            }
        }
        
        public static DateTime Today(int hour, int min, DateTime dayChangeStandardTime = default)
        {
            // validate hour and min
            if (hour < 0) hour = 0;
            else if (hour > 23) hour = 23;

            if (min < 0) min = 0;
            else if (min > 59) min = 59;

            DateTime result = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, min, 0);

            if (dayChangeStandardTime != default)
            {
                bool dayChange = result < dayChangeStandardTime;
                if (dayChange) result = result.AddDays(1);
            }
            return result;
        }
    }
}