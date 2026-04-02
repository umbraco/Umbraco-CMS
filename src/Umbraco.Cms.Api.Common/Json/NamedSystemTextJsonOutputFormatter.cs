using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Umbraco.Cms.Api.Common.Json;

/// <summary>
///     A JSON output formatter that only processes responses for endpoints with matching named JSON options.
/// </summary>
internal sealed class NamedSystemTextJsonOutputFormatter : SystemTextJsonOutputFormatter
{
    private readonly string _jsonOptionsName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NamedSystemTextJsonOutputFormatter"/> class.
    /// </summary>
    /// <param name="jsonOptionsName">The name of the JSON options configuration this formatter handles.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    public NamedSystemTextJsonOutputFormatter(string jsonOptionsName, JsonSerializerOptions jsonSerializerOptions) : base(jsonSerializerOptions)
    {
        _jsonOptionsName = jsonOptionsName;
    }

    /// <inheritdoc/>
    public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        => context.HttpContext.CurrentJsonOptionsName() == _jsonOptionsName && base.CanWriteResult(context);
}
