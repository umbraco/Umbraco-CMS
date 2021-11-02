using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    public interface IContentVersionService
    {
        /// <summary>
        /// Removes historic content versions according to a policy.
        /// </summary>
        IReadOnlyCollection<HistoricContentVersionMeta> PerformContentVersionCleanup(DateTime asAtDate);
    }
}
