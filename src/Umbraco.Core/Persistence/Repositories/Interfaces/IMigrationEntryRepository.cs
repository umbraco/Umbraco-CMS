using Semver;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMigrationEntryRepository : IUnitOfWorkRepository, IQueryRepository<int, IMigrationEntry>
    {
        IMigrationEntry FindEntry(string migrationName, SemVersion version);
    }
}
