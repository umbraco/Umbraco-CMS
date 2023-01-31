using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Api.Common.Json;

public class NamedSystemTextJsonInputFormatter : SystemTextJsonInputFormatter
{
    private readonly string _jsonOptionsName;

    public NamedSystemTextJsonInputFormatter(string jsonOptionsName, JsonOptions options, ILogger<NamedSystemTextJsonInputFormatter> logger)
        : base(options, logger) =>
        _jsonOptionsName = jsonOptionsName;

    public override bool CanRead(InputFormatterContext context)
        => context.HttpContext.CurrentJsonOptionsName() == _jsonOptionsName && base.CanRead(context);

    public override async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
    {
        try
        {
            return await base.ReadAsync(context);
        }
        catch(NotSupportedException e) // E.g. when trying to deserialize to a interface, without sending the $type as part of the request
        {
            context.ModelState.TryAddModelException(string.Empty,new InputFormatterException(e.Message, e));
            return await InputFormatterResult.FailureAsync();
        }
    }
}
