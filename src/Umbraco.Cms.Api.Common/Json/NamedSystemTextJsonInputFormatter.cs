﻿using Microsoft.AspNetCore.Mvc;
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
}
