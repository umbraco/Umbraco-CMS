using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class DateTimeUnspecifiedPropertyIndexValueFactory : DateTimePropertyIndexValueFactory, IDateTimeUnspecifiedPropertyIndexValueFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeUnspecifiedPropertyIndexValueFactory"/> class.
    /// </summary>
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
