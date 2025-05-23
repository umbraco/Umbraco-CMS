using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class SiteDynamicRootOriginFinder : RootDynamicRootOriginFinder
{
    private readonly IEntityService _entityService;
    private readonly IDomainService _domainService;

    public SiteDynamicRootOriginFinder(IEntityService entityService, IDomainService domainService) : base(entityService)
    {
        _entityService = entityService;
        _domainService = domainService;
    }

    protected override string SupportedOriginType { get; set; } = "Site";

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
