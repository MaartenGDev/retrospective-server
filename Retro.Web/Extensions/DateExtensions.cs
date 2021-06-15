using System;

namespace Retro.Web.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfTheDay(this DateTime d) => new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
        public static DateTime EndOfTheDay(this DateTime d) => new DateTime(d.Year, d.Month, d.Day, 23, 59, 59);
    }
}