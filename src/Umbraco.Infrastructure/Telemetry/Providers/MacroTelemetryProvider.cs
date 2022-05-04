using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class MacroTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IMacroService _macroService;

    public MacroTelemetryProvider(IMacroService macroService) => _macroService = macroService;

    public IEnumerable<UsageInformation> GetInformation()
    {
        var macros = _macroService.GetAll().Count();
        yield return new UsageInformation(Constants.Telemetry.MacroCount, macros);
    }
}
