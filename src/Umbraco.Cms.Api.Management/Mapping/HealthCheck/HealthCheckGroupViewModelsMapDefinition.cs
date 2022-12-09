using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.HealthCheck;

public class HealthCheckGroupViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<HealthCheckStatus, HealthCheckResultViewModel>((source, context) => new HealthCheckResultViewModel() { Message = string.Empty }, Map);
        mapper.Define<Core.HealthChecks.HealthCheck, HealthCheckViewModel>((source, context) => new HealthCheckViewModel() { Name = string.Empty }, Map);
        mapper.Define<IGrouping<string?, Core.HealthChecks.HealthCheck>, HealthCheckGroupViewModel>((source, context) => new HealthCheckGroupViewModel(), Map);
        mapper.Define<IEnumerable<IGrouping<string?, Core.HealthChecks.HealthCheck>>, PagedViewModel<HealthCheckGroupViewModel>>((source, context) => new PagedViewModel<HealthCheckGroupViewModel>(), Map);
    }

    private static void Map(HealthCheckStatus source, HealthCheckResultViewModel target, MapperContext context)
    {
        target.Message = source.Message;
        target.ResultType = source.ResultType;
        target.ReadMoreLink = source.ReadMoreLink;
    }

    private static void Map(Core.HealthChecks.HealthCheck source, HealthCheckViewModel target, MapperContext context)
    {
        target.Key = source.Id;
        target.Name = source.Name;
        target.Description = source.Description;
    }

    private static void Map(IGrouping<string?, Core.HealthChecks.HealthCheck> source, HealthCheckGroupViewModel target, MapperContext context)
    {
        target.Name = source.Key;
        target.Checks = context.MapEnumerable<Core.HealthChecks.HealthCheck, HealthCheckViewModel>(source.OrderBy(x => x.Name));
    }

    private static void Map(IEnumerable<IGrouping<string?, Core.HealthChecks.HealthCheck>> source, PagedViewModel<HealthCheckGroupViewModel> target, MapperContext context)
    {
        target.Items = context.MapEnumerable<IGrouping<string?, Core.HealthChecks.HealthCheck>, HealthCheckGroupViewModel>(source);
        target.Total = source.Count();
    }
}
