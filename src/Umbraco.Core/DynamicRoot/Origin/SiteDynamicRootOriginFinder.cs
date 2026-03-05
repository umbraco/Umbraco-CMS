using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

/// <summary>
///     An origin finder that locates the nearest ancestor content item with an assigned domain (site root).
///     This finder handles the "Site" origin type and extends <see cref="RootDynamicRootOriginFinder"/>,
///     falling back to the content tree root if no domain is found.
/// </summary>
public class SiteDynamicRootOriginFinder : RootDynamicRootOriginFinder
{
    private readonly IEntityService _entityService;
    private readonly IDomainService _domainService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SiteDynamicRootOriginFinder"/> class.
    /// </summary>
    /// <param name="entityService">The entity service used to retrieve entities and traverse the content tree.</param>
    /// <param name="domainService">The domain service used to check for assigned domains on content items.</param>
    public SiteDynamicRootOriginFinder(IEntityService entityService, IDomainService domainService) : base(entityService)
    {
        _entityService = entityService;
        _domainService = domainService;
    }

    /// <inheritdoc/>
    protected override string SupportedOriginType { get; set; } = "Site";

    /// <inheritdoc/>
    public override Guid? FindOriginKey(DynamicRootNodeQuery query)
    {
        if (query.OriginAlias != SupportedOriginType)
        {
            return null;
        }

        // when creating new content, CurrentKey will be null - fallback to using ParentKey
        Guid entityKey = query.Context.CurrentKey ?? query.Context.ParentKey;
        IEntitySlim? entity = _entityService.Get(entityKey);
        if (entity is null || entity.NodeObjectType != Constants.ObjectTypes.Document)
        {
            return null;
        }

        string[] contentIdStrings = entity.Path.Split(',');
        for (int i = contentIdStrings.Length - 1; i >= 0; i--)
        {
            var contentId = int.Parse(contentIdStrings[i], NumberStyles.Integer, CultureInfo.InvariantCulture);
            IEnumerable<IDomain> domains = _domainService.GetAssignedDomains(contentId, true);
            if (!domains.Any())
            {
                continue;
            }

            IEntitySlim? entityWithDomain = _entityService.Get(contentId);
            if (entityWithDomain is not null)
            {
                return entityWithDomain.Key;
            }
        }

        // No domains assigned, we fall back to root.
        return base.FindOriginKey(query);
    }
}
