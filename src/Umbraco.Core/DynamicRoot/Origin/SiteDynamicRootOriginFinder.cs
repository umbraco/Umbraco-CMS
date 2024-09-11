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
        if (query.OriginAlias != SupportedOriginType || query.Context.CurrentKey.HasValue is false)
        {
            return null;
        }

        IEntitySlim? entity = _entityService.Get(query.Context.CurrentKey.Value);
        if (entity is null || entity.NodeObjectType != Constants.ObjectTypes.Document)
        {
            return null;
        }


        IEnumerable<string> reversePath = entity.Path.Split(",").Reverse();
        foreach (var contentIdString in reversePath)
        {
            var contentId = int.Parse(contentIdString, NumberStyles.Integer, CultureInfo.InvariantCulture);
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
