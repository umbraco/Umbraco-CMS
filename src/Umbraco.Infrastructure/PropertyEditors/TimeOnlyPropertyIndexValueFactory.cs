using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides a factory for creating property index values for time only datetimes.
/// </summary>
/// <remarks>
/// This is one of four property index value factories derived from <see cref="DateTimePropertyIndexValueFactory"/> and storing their
/// value as JSON with timezone information.
/// </remarks>
internal class TimeOnlyPropertyIndexValueFactory : DateTimePropertyIndexValueFactory, ITimeOnlyPropertyIndexValueFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeOnlyPropertyIndexValueFactory"/> class.
    /// </summary>
    public TimeOnlyPropertyIndexValueFactory(
        IJsonSerializer jsonSerializer,
        ILogger<TimeOnlyPropertyIndexValueFactory> logger)
        : base(jsonSerializer, logger)
    {
    }

    /// <inheritdoc/>
    protected override string MapDateToIndexValueFormat(DateTimeOffset date)
        => date.UtcDateTime.ToString("HH:mm:ss");
}
