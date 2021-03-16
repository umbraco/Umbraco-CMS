using System.Collections.Generic;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Core.Events
{
    public class UninstallPackageNotification : ICancelableNotification
    {
        public UninstallPackageNotification(IEnumerable<UninstallationSummary> uninstallationSummary) => UninstallationSummary = uninstallationSummary;

        public IEnumerable<UninstallationSummary> UninstallationSummary { get; }

        public bool Cancel { get; set; }
    }
}


