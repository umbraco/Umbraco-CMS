using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Allows for outputting a normalized appdomainappid token in a log format
    /// </summary>
    public sealed class AppDomainTokenConverter : log4net.Util.PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            writer.Write(HttpRuntime.AppDomainAppId.ReplaceNonAlphanumericChars(string.Empty));
        }
    }
}
