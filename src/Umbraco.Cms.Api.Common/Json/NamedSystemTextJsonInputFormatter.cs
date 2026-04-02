using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Api.Common.Json;

/// <summary>
///     A JSON input formatter that only processes requests for endpoints with matching named JSON options.
/// </summary>
internal sealed class NamedSystemTextJsonInputFormatter : SystemTextJsonInputFormatter
{
    private readonly string _jsonOptionsName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NamedSystemTextJsonInputFormatter"/> class.
    /// </summary>
    /// <param name="jsonOptionsName">The name of the JSON options configuration this formatter handles.</param>
    /// <param name="options">The JSON options.</param>
    /// <param name="logger">The logger.</param>
    public NamedSystemTextJsonInputFormatter(string jsonOptionsName, JsonOptions options, ILogger<NamedSystemTextJsonInputFormatter> logger)
        : base(options, logger) =>
        _jsonOptionsName = jsonOptionsName;

    /// <inheritdoc/>
    public override bool CanRead(InputFormatterContext context)
        => context.HttpContext.CurrentJsonOptionsName() == _jsonOptionsName && base.CanRead(context);

    /// <inheritdoc/>
    public override async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
    {
        try
        {
            return await base.ReadAsync(context);
        }
        catch (NotSupportedException exception)
        {
            // This happens when trying to deserialize to an interface, without sending the $type as part of the request
            context.ModelState.TryAddModelException(string.Empty, new InputFormatterException(exception.Message, exception));
            return await InputFormatterResult.FailureAsync();
        }
    }
}
