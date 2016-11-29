using System;
using Semver;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMigrationEntryRepository : IQueryRepository<int, IMigrationEntry>
    {
        IMigrationEntry FindEntry(string migrationName, SemVersion version);
    }
}