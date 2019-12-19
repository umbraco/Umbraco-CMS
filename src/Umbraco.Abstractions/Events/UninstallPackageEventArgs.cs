using System.Collections.Generic;
using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Events
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
