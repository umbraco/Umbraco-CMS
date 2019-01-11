using System.Collections.Generic;
using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Events
{
    public class UninstallPackageEventArgs<TEntity> : CancellableObjectEventArgs<IEnumerable<TEntity>>
    {
        public UninstallPackageEventArgs(TEntity eventObject, IPackageInfo packageMetaData, bool canCancel)
            : base(new[] { eventObject }, canCancel)
        {
            PackageMetaData = packageMetaData;
        }

        public IPackageInfo PackageMetaData { get; }

        public IEnumerable<TEntity> UninstallationSummary => EventObject;
    }
}
