using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Filters;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiController]
[ApiVersion("1.0")]
[DeliveryApiAccess]
[JsonOptionsName(Constants.JsonOptionsNames.DeliveryApi)]
[LocalizeFromAcceptLanguageHeader]
[MapToApi(DeliveryApiConfiguration.ApiName)]
public abstract class DeliveryApiControllerBase : Controller
{
}
