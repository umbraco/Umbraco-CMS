using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core
{
    public static class DateTimeExtensions
    {

        /// <summary>
        /// Returns the DateTime as an ISO formatted string that is globally expectable
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToIsoString(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

    }
}
