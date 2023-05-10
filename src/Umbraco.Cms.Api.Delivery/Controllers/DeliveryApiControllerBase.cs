using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Filters;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiController]
[DeliveryApiAccess]
[JsonOptionsName(Constants.JsonOptionsNames.DeliveryApi)]
[MapToApi(DeliveryApiConfiguration.ApiName)]
public abstract class DeliveryApiControllerBase : Controller
{
}
