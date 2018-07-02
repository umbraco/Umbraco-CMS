using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Events
{
    public class ImportPackageEventArgs<TEntity> : CancellableEnumerableObjectEventArgs<TEntity>, IEquatable<ImportPackageEventArgs<TEntity>>
    {
        private readonly MetaData _packageMetaData;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the overload specifying packageMetaData instead")]
        public ImportPackageEventArgs(TEntity eventObject, bool canCancel)
            : base(new[] { eventObject }, canCancel)
        {
        }

        public ImportPackageEventArgs(TEntity eventObject, MetaData packageMetaData, bool canCancel)
            : base(new[] { eventObject }, canCancel)
        {
            if (packageMetaData == null) throw new ArgumentNullException("packageMetaData");
            _packageMetaData = packageMetaData;
        }

        public ImportPackageEventArgs(TEntity eventObject, MetaData packageMetaData)
            : this(eventObject, packageMetaData, true)
        {
            
        }

        public MetaData PackageMetaData
        {
            get { return _packageMetaData; }
        }

        public IEnumerable<TEntity> InstallationSummary
        {
            get { return EventObject; }
        }

        public bool Equals(ImportPackageEventArgs<TEntity> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            //TODO: MetaData for package metadata has no equality operators :/
            return base.Equals(other) && _packageMetaData.Equals(other._packageMetaData);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ImportPackageEventArgs<TEntity>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ _packageMetaData.GetHashCode();
            }
        }

        public static bool operator ==(ImportPackageEventArgs<TEntity> left, ImportPackageEventArgs<TEntity> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ImportPackageEventArgs<TEntity> left, ImportPackageEventArgs<TEntity> right)
        {
            return !Equals(left, right);
        }
    }
}
