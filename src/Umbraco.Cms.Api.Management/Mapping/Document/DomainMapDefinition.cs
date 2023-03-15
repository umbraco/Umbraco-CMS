using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using CoreDomainModel = Umbraco.Cms.Core.Models.ContentEditing.DomainModel;

namespace Umbraco.Cms.Api.Management.Mapping.Document;

public class DomainMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IEnumerable<IDomain>, DomainsResponseModel>((_, _) => new DomainsResponseModel { Domains = Enumerable.Empty<DomainPresentationModel>() }, Map);
        mapper.Define<UpdateDomainsRequestModel, DomainsUpdateModel>((_, _) => new DomainsUpdateModel { Domains = Enumerable.Empty<CoreDomainModel>() }, Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IEnumerable<IDomain> source, DomainsResponseModel target, MapperContext context)
    {
        IDomain[] sourceAsArray = source.ToArray();
        IDomain[] wildcardsDomains = sourceAsArray.Where(d => d.IsWildcard).ToArray();

        target.DefaultIsoCode = wildcardsDomains.FirstOrDefault()?.LanguageIsoCode;
        target.Domains = sourceAsArray.Except(wildcardsDomains).Select(domain => new DomainPresentationModel
        {
            DomainName = domain.DomainName,
            IsoCode = domain.LanguageIsoCode ?? string.Empty
        }).ToArray();
    }

    private void Map(UpdateDomainsRequestModel source, DomainsUpdateModel target, MapperContext context)
    {
        target.DefaultIsoCode = source.DefaultIsoCode;
        target.Domains = source.Domains.Select(domain => new Core.Models.ContentEditing.DomainModel
        {
            DomainName = domain.DomainName,
            IsoCode = domain.IsoCode
        }).ToArray();
    }
}
