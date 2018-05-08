using System.Collections.Generic;
using Umbraco.Core.Packaging.Models;

namespace Umbraco.Core.Events
{
    public class UninstallPackageEventArgs<TEntity> : CancellableObjectEventArgs<IEnumerable<TEntity>>
    {
        private readonly MetaData _packageMetaData;

        public UninstallPackageEventArgs(TEntity eventObject, bool canCancel)
            : base(new[] { eventObject }, canCancel)
        {
        }

        public UninstallPackageEventArgs(TEntity eventObject, MetaData packageMetaData)
            : base(new[] { eventObject })
        {
            _packageMetaData = packageMetaData;
        }

        public MetaData PackageMetaData
        {
            get { return _packageMetaData; }
        }

        public IEnumerable<TEntity> UninstallationSummary
        {
            get { return EventObject; }
        }
    }
}