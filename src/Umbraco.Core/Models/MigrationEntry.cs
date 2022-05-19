using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Models;

public class MigrationEntry : EntityBase, IMigrationEntry
{
    private string? _migrationName;
    private SemVersion? _version;

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

    public string? MigrationName
    {
        get => _migrationName;
        set => SetPropertyValueAndDetectChanges(value, ref _migrationName, nameof(MigrationName));
    }

    public SemVersion? Version
    {
        get => _version;
        set => SetPropertyValueAndDetectChanges(value, ref _version, nameof(Version));
    }
}
