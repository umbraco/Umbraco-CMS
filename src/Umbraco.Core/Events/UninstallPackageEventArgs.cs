using System.Collections.Generic;
using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Events
{
    public class UninstallPackageEventArgs<TEntity> : CancellableObjectEventArgs<IEnumerable<TEntity>>
    {
        public UninstallPackageEventArgs(TEntity eventObject, bool canCancel)
            : base(new[] { eventObject }, canCancel)
        { }

        public UninstallPackageEventArgs(TEntity eventObject, MetaData packageMetaData)
            : base(new[] { eventObject })
        {
            PackageMetaData = packageMetaData;
        }

        public MetaData PackageMetaData { get; }

        public IEnumerable<TEntity> UninstallationSummary => EventObject;
    }
}
