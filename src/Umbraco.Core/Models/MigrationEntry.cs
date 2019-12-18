using System;
using Semver;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public class MigrationEntry : EntityBase, IMigrationEntry
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

        private string _migrationName;
        private SemVersion _version;

        public string MigrationName
        {
            get => _migrationName;
            set => SetPropertyValueAndDetectChanges(value, ref _migrationName, nameof(MigrationName));
        }

        public SemVersion Version
        {
            get => _version;
            set => SetPropertyValueAndDetectChanges(value, ref _version, nameof(Version));
        }
    }
}
