using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace Umbraco.Cms.Core.Logging;

public class MessageTemplates : IMessageTemplates
{
    // Umbraco now uses Message Templates (https://messagetemplates.org/) for logging, which means
    // we cannot plainly use string.Format() to format them. There is a work-in-progress C# lib,
    // derived from Serilog, which should help (https://github.com/messagetemplates/messagetemplates-csharp)
    // but it only has a pre-release NuGet package. So, we've got to use Serilog's code, which
    // means we cannot get rid of Serilog entirely. We may want to revisit this at some point.

    // TODO: Do we still need this, is there a non-pre release package shipped?
    private static readonly Lazy<ILogger> _minimalLogger = new(() => new LoggerConfiguration().CreateLogger());

    public string Render(string messageTemplate, params object[] args)
    {
        // resolve a minimal logger instance which is used to bind message templates
        ILogger logger = _minimalLogger.Value;

        var bound = logger.BindMessageTemplate(messageTemplate, args, out MessageTemplate? parsedTemplate, out IEnumerable<LogEventProperty>? boundProperties);

        if (!bound)
        {
            throw new FormatException($"Could not format message \"{messageTemplate}\" with {args.Length} args.");
        }

        var values = boundProperties!.ToDictionary(x => x.Name, x => x.Value);

        // this ends up putting every string parameter between quotes
        // return parsedTemplate.Render(values);

        // this does not
        var tw = new StringWriter();
        foreach (MessageTemplateToken? t in parsedTemplate!.Tokens)
        {
            if (t is PropertyToken pt &&
                values.TryGetValue(pt.PropertyName, out LogEventPropertyValue? propVal) &&
                (propVal as ScalarValue)?.Value is string s)
            {
                tw.Write(s);
            }
            else
            {
                t.Render(values, tw);
            }
        }

        return tw.ToString();
    }
}
