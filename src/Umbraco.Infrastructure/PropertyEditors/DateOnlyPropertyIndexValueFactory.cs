using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class DateOnlyPropertyIndexValueFactory : DateTimePropertyIndexValueFactory, IDateOnlyPropertyIndexValueFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateOnlyPropertyIndexValueFactory"/> class.
    /// </summary>
    public DateOnlyPropertyIndexValueFactory(
        IJsonSerializer jsonSerializer,
        ILogger<DateTimePropertyIndexValueFactory> logger)
        : base(jsonSerializer, logger)
    {
    }

    /// <inheritdoc/>
    protected override string MapDateToIndexValueFormat(DateTimeOffset date)
        => date.UtcDateTime.ToString("yyyy-MM-dd");
}
