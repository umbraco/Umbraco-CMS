using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Models;

public interface IMigrationEntry : IEntity, IRememberBeingDirty
{
    string? MigrationName { get; set; }

    SemVersion? Version { get; set; }
}
