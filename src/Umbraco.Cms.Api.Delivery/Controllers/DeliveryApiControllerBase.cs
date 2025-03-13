using System.Net;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiController]
[JsonOptionsName(Constants.JsonOptionsNames.DeliveryApi)]
[MapToApi(DeliveryApiConfiguration.ApiName)]
public abstract class DeliveryApiControllerBase : Controller
{
    protected string DecodePath(string path)
    {
        // OpenAPI does not allow reserved chars as "in:path" parameters, so clients based on the Swagger JSON will URL
        // encode the path. Normally, ASP.NET Core handles that encoding with an automatic decoding - apparently just not
        // for forward slashes, for whatever reason... so we need to deal with those. Hopefully this will be addressed in
        // an upcoming version of ASP.NET Core.
        // See also https://github.com/dotnet/aspnetcore/issues/11544
        if (path.Contains("%2F", StringComparison.OrdinalIgnoreCase))
        {
            path = WebUtility.UrlDecode(path);
        }

        return path;
    }
}
