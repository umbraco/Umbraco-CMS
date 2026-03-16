using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides a factory for creating property index values for unspecified dates.
/// </summary>
/// <remarks>
/// This is one of four property index value factories derived from <see cref="DateTimePropertyIndexValueFactory"/> and storing their
/// value as JSON with timezone information.
/// </remarks>
internal class DateTimeUnspecifiedPropertyIndexValueFactory : DateTimePropertyIndexValueFactory, IDateTimeUnspecifiedPropertyIndexValueFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeUnspecifiedPropertyIndexValueFactory"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The serializer used for JSON operations.</param>
    /// <param name="logger">The logger used for logging information and errors.</param>
    public DateTimeUnspecifiedPropertyIndexValueFactory(
        IJsonSerializer jsonSerializer,
        ILogger<DateTimeUnspecifiedPropertyIndexValueFactory> logger)
        : base(jsonSerializer, logger)
    {
    }

    /// <inheritdoc/>
    protected override string MapDateToIndexValueFormat(DateTimeOffset date)
        => date.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
}
