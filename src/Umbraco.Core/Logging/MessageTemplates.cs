using System;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Provides tools to support message templates.
    /// </summary>
    public static class MessageTemplates
    {
        // Umbraco now uses Message Templates (https://messagetemplates.org/) for logging, which means
        // we cannot plainly use string.Format() to format them. There is a work-in-progress C# lib,
        // derived from Serilog, which should help (https://github.com/messagetemplates/messagetemplates-csharp)
        // but it only has a pre-release NuGet package. So, we've got to use Serilog's code, which
        // means we cannot get rid of Serilog entirely. We may want to revisit this at some point.

        private static readonly Lazy<global::Serilog.ILogger> MinimalLogger = new Lazy<global::Serilog.ILogger>(() => new LoggerConfiguration().CreateLogger());

        public static string Render(string messageTemplate, params object[] args)
        {
            // by default, unless initialized otherwise, Log.Logger is SilentLogger which cannot bind message
            // templates. Log.Logger is set to a true Logger when initializing Umbraco's logger, but in case
            // that has not been done already - use a temp minimal logger (eg for tests).
            var logger = Log.Logger as global::Serilog.Core.Logger ?? MinimalLogger.Value;

            var bound = logger.BindMessageTemplate(messageTemplate, args, out var parsedTemplate, out var boundProperties);

            if (!bound)
                throw new FormatException($"Could not format message \"{messageTemplate}\" with {args.Length} args.");

            var values = boundProperties.ToDictionary(x => x.Name, x => x.Value);

            // this ends up putting every string parameter between quotes
            //return parsedTemplate.Render(values);

            // this does not
            var tw = new StringWriter();
            foreach (var t in parsedTemplate.Tokens)
            {
                if (t is PropertyToken pt &&
                    values.TryGetValue(pt.PropertyName, out var propVal) &&
                    (propVal as ScalarValue)?.Value is string s)
                    tw.Write(s);
                else
                    t.Render(values, tw);
            }
            return tw.ToString();
        }
    }
}
