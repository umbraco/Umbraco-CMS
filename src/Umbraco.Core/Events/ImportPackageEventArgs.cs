using System.Collections.Generic;
using Umbraco.Core.Packaging.Models;

namespace Umbraco.Core.Events
{
    internal class ImportPackageEventArgs<TEntity> : CancellableObjectEventArgs<IEnumerable<TEntity>>
    {
        private readonly MetaData _packageMetaData;

        public ImportPackageEventArgs(TEntity eventObject, bool canCancel)
            : base(new[] { eventObject }, canCancel)
        {
        }

        public ImportPackageEventArgs(TEntity eventObject, MetaData packageMetaData)
            : base(new[] { eventObject })
        {
            _packageMetaData = packageMetaData;
        }

        public MetaData PackageMetaData
        {
            get { return _packageMetaData; }
        }
    }
}