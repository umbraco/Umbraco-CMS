using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class MacroTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly IMacroService _macroService;

        public MacroTelemetryProvider(IMacroService macroService)
        {
            _macroService = macroService;
        }

        public IEnumerable<UsageInformation> GetInformation()
        {
            var macros = _macroService.GetAll().Count();
            yield return new UsageInformation(Constants.Telemetry.MacroCount, macros);
        }
    }
}
