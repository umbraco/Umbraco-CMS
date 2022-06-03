using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    /// <summary>
    /// A no-op implementation of <see cref="IMemberTypeContainerRepository"/>, as containers aren't supported for members.
    /// </summary>
    /// <remarks>
    /// Introduced to avoid inconsistencies with nullability of dependencies for type repositories for content, media and members.
    /// </remarks>
    internal class MemberTypeContainerRepository : IMemberTypeContainerRepository
    {
        public void Delete(EntityContainer entity)
        {
        }

        public bool Exists(int id) => false;
        public Task<bool> ExistsAsync(int id) => Task.FromResult(false);

        public EntityContainer? Get(Guid id) => null;

        public IEnumerable<EntityContainer> Get(string name, int level) => Enumerable.Empty<EntityContainer>();

        public EntityContainer? Get(int id) => null;
        public Task<EntityContainer?> GetAsync(int id) => Task.FromResult<EntityContainer?>(null);
        public IEnumerable<EntityContainer> GetMany(params int[]? ids) => Enumerable.Empty<EntityContainer>();

        public void Save(EntityContainer entity)
        {
        }

        public Task SaveAsync(EntityContainer entity) => Task.CompletedTask;
    }
}
