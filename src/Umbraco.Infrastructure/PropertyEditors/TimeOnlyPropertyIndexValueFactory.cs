using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

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
