using Semver;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMigrationEntryRepository : IUnitOfWorkRepository, IReadWriteQueryRepository<int, IMigrationEntry>
    {
        IMigrationEntry FindEntry(string migrationName, SemVersion version);
    }
}
