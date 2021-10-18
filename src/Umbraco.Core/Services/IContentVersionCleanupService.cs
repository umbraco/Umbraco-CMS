using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IContentVersionCleanupService
    {
        /// <summary>
        /// Removes historic content versions according to a policy.
        /// </summary>
        IReadOnlyCollection<HistoricContentVersionMeta> PerformContentVersionCleanup(DateTime asAtDate);
    }
}
