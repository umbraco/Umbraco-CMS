using Serilog.Events;

namespace Umbraco.Cms.Infrastructure.Logging.Viewer;

public class SerilogExpressionsFunctions
{
    // This Has() code is the same as the renamed IsDefined() function
    // Added this to help backport and ensure saved queries continue to work if using Has()
    public static LogEventPropertyValue? Has(LogEventPropertyValue? value) => new ScalarValue(value != null);
}
