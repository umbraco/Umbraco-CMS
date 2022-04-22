using Microsoft.AspNetCore.DataProtection.Infrastructure;
using Serilog.Core;
using Serilog.Events;
using Umbraco.Cms.Web.Common.Extensions;

namespace Umbraco.Cms.Web.Common.Logging.Enrichers;

internal class ApplicationIdEnricher : ILogEventEnricher
{
    private readonly IApplicationDiscriminator _applicationDiscriminator;
    public const string ApplicationIdProperty = "ApplicationId";

    public ApplicationIdEnricher(IApplicationDiscriminator applicationDiscriminator) =>
        _applicationDiscriminator = applicationDiscriminator;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) =>
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(ApplicationIdProperty, _applicationDiscriminator.GetApplicationId()));
}
