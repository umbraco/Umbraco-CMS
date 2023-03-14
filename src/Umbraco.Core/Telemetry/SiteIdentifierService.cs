using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Telemetry;

/// <inheritdoc />
internal class SiteIdentifierService : ISiteIdentifierService
{
    private readonly IConfigManipulator _configManipulator;
    private readonly ILogger<SiteIdentifierService> _logger;
    private GlobalSettings _globalSettings;

    public SiteIdentifierService(
        IOptionsMonitor<GlobalSettings> optionsMonitor,
        IConfigManipulator configManipulator,
        ILogger<SiteIdentifierService> logger)
    {
        _globalSettings = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange(globalSettings => _globalSettings = globalSettings);
        _configManipulator = configManipulator;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool TryGetSiteIdentifier(out Guid siteIdentifier)
    {
        // Parse telemetry string as a GUID & verify its a GUID and not some random string
        // since users may have messed with or decided to empty the app setting or put in something random
        if (Guid.TryParse(_globalSettings.Id, out Guid parsedTelemetryId) is false
            || parsedTelemetryId == Guid.Empty)
        {
            siteIdentifier = Guid.Empty;
            return false;
        }

        siteIdentifier = parsedTelemetryId;
        return true;
    }

    /// <inheritdoc />
    public bool TryGetOrCreateSiteIdentifier(out Guid siteIdentifier)
    {
        if (TryGetSiteIdentifier(out Guid existingId))
        {
            siteIdentifier = existingId;
            return true;
        }

        if (TryCreateSiteIdentifier(out Guid createdId))
        {
            siteIdentifier = createdId;
            return true;
        }

        siteIdentifier = Guid.Empty;
        return false;
    }

    /// <inheritdoc />
    public bool TryCreateSiteIdentifier(out Guid createdGuid)
    {
        createdGuid = Guid.NewGuid();

        try
        {
            _configManipulator.SetGlobalId(createdGuid.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't update config files with a telemetry site identifier");
            createdGuid = Guid.Empty;
            return false;
        }

        return true;
    }
}
