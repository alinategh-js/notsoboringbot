using System;
using System.Collections.Generic;
using System.Text;

namespace NotSoBoring.Domain.Utils
{
    public static class TimeSpanUtils
    {
        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} Days, ", span.Days) : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} Hours, ", span.Hours) : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} Minutes, ", span.Minutes) : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} Seconds", span.Seconds) : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 Seconds";

            return formatted;
        }
    }
}
