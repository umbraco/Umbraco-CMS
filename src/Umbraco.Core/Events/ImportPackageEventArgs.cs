using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Events
{
    public class ImportPackageEventArgs<TEntity> : CancellableEnumerableObjectEventArgs<TEntity>, IEquatable<ImportPackageEventArgs<TEntity>>
    {
        public ImportPackageEventArgs(TEntity eventObject, MetaData packageMetaData, bool canCancel)
            : base(new[] { eventObject }, canCancel)
        {
            PackageMetaData = packageMetaData ?? throw new ArgumentNullException(nameof(packageMetaData));
        }

        public ImportPackageEventArgs(TEntity eventObject, MetaData packageMetaData)
            : this(eventObject, packageMetaData, true)
        {
            
        }

        public MetaData PackageMetaData { get; }

        public IEnumerable<TEntity> InstallationSummary => EventObject;

        public bool Equals(ImportPackageEventArgs<TEntity> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            //TODO: MetaData for package metadata has no equality operators :/
            return base.Equals(other) && PackageMetaData.Equals(other.PackageMetaData);
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
                return (base.GetHashCode() * 397) ^ PackageMetaData.GetHashCode();
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
