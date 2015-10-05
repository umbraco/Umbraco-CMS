using System;
using System.Collections.Generic;
using Semver;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMigrationEntryRepository : IRepositoryQueryable<int, IMigrationEntry>
    {
        IMigrationEntry FindEntry(string migrationName, SemVersion version);

        IEnumerable<IMigrationEntry> FindEntries(SemVersion version, params string[] migrationNames);

        IEnumerable<IMigrationEntry> FindEntries(string migrationName);
    }
}