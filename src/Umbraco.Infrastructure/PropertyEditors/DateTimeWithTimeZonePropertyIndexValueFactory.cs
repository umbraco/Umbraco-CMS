using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides a factory for creating property index values for datetimes stored with timezones.
/// </summary>
/// <remarks>
/// This is one of four property index value factories derived from <see cref="DateTimePropertyIndexValueFactory"/> and storing their
/// value as JSON with timezone information.
/// </remarks>
internal class DateTimeWithTimeZonePropertyIndexValueFactory : DateTimePropertyIndexValueFactory, IDateTimeWithTimeZonePropertyIndexValueFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeWithTimeZonePropertyIndexValueFactory"/> class.
    /// </summary>
    public DateTimeWithTimeZonePropertyIndexValueFactory(
        IJsonSerializer jsonSerializer,
        ILogger<DateTimeWithTimeZonePropertyIndexValueFactory> logger)
        : base(jsonSerializer, logger)
    {
    }

    /// <inheritdoc/>
    protected override string MapDateToIndexValueFormat(DateTimeOffset date)
        => date.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
}
