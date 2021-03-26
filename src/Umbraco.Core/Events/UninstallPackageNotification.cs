using System.Collections.Generic;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Core.Events
{
    public class UninstallPackageNotification : INotification
    {
        public UninstallPackageNotification(IEnumerable<UninstallationSummary> uninstallationSummary) => UninstallationSummary = uninstallationSummary;

        public IEnumerable<UninstallationSummary> UninstallationSummary { get; }
    }
}


