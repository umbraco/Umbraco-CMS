using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Umbraco.Extensions;

public static class ActionResultExtensions
{
    public static bool IsSuccessStatusCode(this ActionResult actionResult)
    {
        var statusCode = actionResult switch
        {
            IStatusCodeActionResult x => x.StatusCode,
            _ => null,
        };

        return statusCode.HasValue && statusCode.Value >= (int)HttpStatusCode.OK &&
               statusCode.Value < (int)HttpStatusCode.Ambiguous;
    }
}
