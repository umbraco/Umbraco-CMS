using System;
using System.Reflection;
using Semver;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public class MigrationEntry : Entity, IMigrationEntry
    {
        public MigrationEntry()
        {
        }

        public MigrationEntry(int id, DateTime createDate, string migrationName, SemVersion version)
        {
            Id = id;
            CreateDate = createDate;
            _migrationName = migrationName;
            _version = version;
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<MigrationEntry, string>(x => x.MigrationName);
            public readonly PropertyInfo VersionSelector = ExpressionHelper.GetPropertyInfo<MigrationEntry, SemVersion>(x => x.Version);
        }

        private string _migrationName;
        private SemVersion _version;

        public string MigrationName
        {
            get { return _migrationName; }
            set { SetPropertyValueAndDetectChanges(value, ref _migrationName, Ps.Value.NameSelector); }
        }

        public SemVersion Version
        {
            get { return _version; }
            set { SetPropertyValueAndDetectChanges(value, ref _version, Ps.Value.VersionSelector); }
        }
    }
}