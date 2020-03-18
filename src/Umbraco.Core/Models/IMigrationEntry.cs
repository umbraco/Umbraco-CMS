using System;
using Semver;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public interface IMigrationEntry : IEntity, IRememberBeingDirty
    {
        string MigrationName { get; set; }
        SemVersion Version { get; set; }
    }
}
