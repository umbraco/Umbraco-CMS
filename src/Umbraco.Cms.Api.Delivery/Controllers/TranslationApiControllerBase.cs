using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Filters;
using Umbraco.Cms.Api.Delivery.Routing;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[DeliveryApiAccess]
[VersionedDeliveryApiRoute("translation")]
[ApiExplorerSettings(GroupName = "Translation")]
[LocalizeFromAcceptLanguageHeader]
public abstract class TranslationApiControllerBase : DeliveryApiControllerBase
{
    public ILocalizationService LocalizationService { get; }
    public IRequestCultureService RequestCultureService { get; }

    protected TranslationApiControllerBase(ILocalizationService localizationService, IRequestCultureService requestCultureService)
    {
        LocalizationService = localizationService;
        RequestCultureService = requestCultureService;
    }

}
