using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

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
