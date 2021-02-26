using System.Collections.Generic;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Core.Events
{
    public class UninstallPackageEventArgs: CancellableObjectEventArgs<IEnumerable<UninstallationSummary>>
    {
        public UninstallPackageEventArgs(IEnumerable<UninstallationSummary> eventObject, bool canCancel)
            : base(eventObject, canCancel)
        {
        }

        public IEnumerable<UninstallationSummary> UninstallationSummary => EventObject;
    }
}
