using Serilog.Events;

namespace Umbraco.Cms.Infrastructure.Logging.Viewer;

/// <summary>
/// Provides utility functions for use within Serilog expression evaluations in the logging viewer.
/// </summary>
public class SerilogExpressionsFunctions
{
    /// <summary>
    /// Determines whether the specified <see cref="LogEventPropertyValue"/> is not null.
    /// </summary>
    /// <remarks>
    /// This Has() code is the same as the renamed IsDefined() function
    /// Added this to help backport and ensure saved queries continue to work if using Has()
    /// </remarks>
    /// <param name="value">The <see cref="LogEventPropertyValue"/> to check for presence.</param>
    /// <returns>A <see cref="ScalarValue"/> containing <c>true</c> if <paramref name="value"/> is not null; otherwise, <c>false</c>.</returns>
    public static LogEventPropertyValue Has(LogEventPropertyValue? value) => new ScalarValue(value != null);
}
