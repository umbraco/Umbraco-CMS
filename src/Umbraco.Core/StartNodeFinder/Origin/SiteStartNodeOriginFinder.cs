using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.StartNodeFinder.Origin;

public class SiteStartNodeOriginFinder : RootStartNodeOriginFinder
{
    private readonly IEntityService _entityService;
    private readonly IDomainService _domainService;

    public SiteStartNodeOriginFinder(IEntityService entityService, IDomainService domainService) : base(entityService)
    {
        _entityService = entityService;
        _domainService = domainService;
    }

    protected override string SupportedOriginType { get; set; } = "Site";

    public override Guid? FindOriginKey(StartNodeSelector selector)
    {
        if (selector.OriginAlias != SupportedOriginType || selector.Context.CurrentKey.HasValue is false)
        {
            return null;
        }

        IEntitySlim? entity = _entityService.Get(selector.Context.CurrentKey.Value);
        if (entity is null || entity.NodeObjectType != Constants.ObjectTypes.Document)
        {
            return null;
        }


        IEnumerable<string> reversePath = entity.Path.Split(",").Reverse();
        foreach (var contentIdString in reversePath)
        {
            var contentId = int.Parse(contentIdString, NumberStyles.Integer, CultureInfo.InvariantCulture);
            IEnumerable<IDomain> domains = _domainService.GetAssignedDomains(contentId, true);
            if (domains.Any())
            {
                IEntitySlim? entityWithDomain = _entityService.Get(contentId);
                if (entityWithDomain is not null)
                {
                    return entityWithDomain.Key;
                }
            }
        }

        // No domains assigned, we fall back to root.
        return base.FindOriginKey(selector);
    }
}
