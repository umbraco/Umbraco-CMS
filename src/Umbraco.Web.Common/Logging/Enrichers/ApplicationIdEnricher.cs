using Microsoft.AspNetCore.DataProtection.Infrastructure;
using Serilog.Core;
using Serilog.Events;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Logging.Enrichers;

internal class ApplicationIdEnricher : ILogEventEnricher
{
    public const string ApplicationIdProperty = "ApplicationId";
    private readonly IApplicationDiscriminator _applicationDiscriminator;

    public ApplicationIdEnricher(IApplicationDiscriminator applicationDiscriminator) =>
        _applicationDiscriminator = applicationDiscriminator;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) =>
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(
            ApplicationIdProperty,
            _applicationDiscriminator.GetApplicationId()));
}
