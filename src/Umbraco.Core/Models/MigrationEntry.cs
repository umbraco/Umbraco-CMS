using System;
using System.Reflection;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public class MigrationEntry : Entity, IMigrationEntry
    {
        public MigrationEntry()
        {
        }

        public MigrationEntry(int id, DateTime createDate, string migrationName, Version version)
        {
            Id = id;
            CreateDate = createDate;
            _migrationName = migrationName;
            _version = version;
        }

        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<MigrationEntry, string>(x => x.MigrationName);
        private static readonly PropertyInfo VersionSelector = ExpressionHelper.GetPropertyInfo<MigrationEntry, Version>(x => x.Version);
        private string _migrationName;
        private Version _version;

        public string MigrationName
        {
            get { return _migrationName; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _migrationName = value;
                    return _migrationName;
                }, _migrationName, NameSelector);
            }
        }

        public Version Version
        {
            get { return _version; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _version = value;
                    return _version;
                }, _version, VersionSelector);
            }
        }
    }
}