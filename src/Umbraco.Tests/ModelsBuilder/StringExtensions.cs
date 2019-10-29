using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.ModelsBuilder.Tests
{
    public static class StringExtensions
    {
        public static string ClearLf(this string s)
        {
            return s.Replace("\r", "");
        }
    }
}
