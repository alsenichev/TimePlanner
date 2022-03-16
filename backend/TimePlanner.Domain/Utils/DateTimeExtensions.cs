namespace TimePlanner.Domain.Utils
{
    public static class DateTimeExtensions
    {
        private static DateTime RoundToNearest(DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;
            var offset = roundUp ? d.Ticks : 0;
            return new DateTime(dt.Ticks + offset - delta, dt.Kind);
        }

        public static DateTime RoundSeconds(this DateTime dt)
        {
          // TODO - move to ITimeProvider
            return RoundToNearest(dt, TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Returns the part of the TimeSpan which is a multiple of 30 minutes.
        /// (Tried to round up to the nearest 30 minutes, but it felt not quite
        /// comfortable with the negative registered time).
        /// </summary>
        public static TimeSpan TaskAssignable(this TimeSpan ts)
        {
            var delta = ts.Ticks % TimeSpan.FromMinutes(30).Ticks;
            return TimeSpan.FromTicks(ts.Ticks - delta);
        }

    }
}
