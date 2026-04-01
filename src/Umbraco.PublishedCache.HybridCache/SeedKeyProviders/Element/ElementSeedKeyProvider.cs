namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Element;

// TODO: Implement breadth-first seeding once IElementNavigationQueryService exists (see MediaBreadthFirstKeyProvider for reference).
internal sealed class ElementSeedKeyProvider : IElementSeedKeyProvider
{
    public ISet<Guid> GetSeedKeys() => new HashSet<Guid>();
}
