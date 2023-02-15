using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Umbraco.Cms.Api.Common.Json;


internal class NamedSystemTextJsonOutputFormatter : SystemTextJsonOutputFormatter
{
    private readonly string _jsonOptionsName;

    public NamedSystemTextJsonOutputFormatter(string jsonOptionsName, JsonSerializerOptions jsonSerializerOptions) : base(jsonSerializerOptions)
    {
        _jsonOptionsName = jsonOptionsName;
    }

    public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        => context.HttpContext.CurrentJsonOptionsName() == _jsonOptionsName && base.CanWriteResult(context);
}
