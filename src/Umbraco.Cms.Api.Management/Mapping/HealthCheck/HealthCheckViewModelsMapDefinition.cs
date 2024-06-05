using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.HealthCheck;

public class HealthCheckViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<HealthCheckActionRequestModel, HealthCheckAction>((_, _) => new HealthCheckAction(), Map);
        mapper.Define<HealthCheckAction, HealthCheckActionRequestModel>((_, _) => new HealthCheckActionRequestModel { ValueRequired = false, HealthCheck = new() }, Map);
        mapper.Define<HealthCheckStatus, HealthCheckResultResponseModel>((_, _) => new HealthCheckResultResponseModel { Message = string.Empty }, Map);
        mapper.Define<Core.HealthChecks.HealthCheck, HealthCheckViewModel>((_, _) => new HealthCheckViewModel { Name = string.Empty }, Map);
        mapper.Define<IGrouping<string?, Core.HealthChecks.HealthCheck>, HealthCheckGroupPresentationModel>(
            (_, _) => new HealthCheckGroupPresentationModel
            {
                Name = string.Empty,
                Checks = new List<HealthCheckViewModel>()
            },
            Map);
        mapper.Define<IGrouping<string?, Core.HealthChecks.HealthCheck>, HealthCheckGroupResponseModel>((_, _) => new HealthCheckGroupResponseModel { Name = string.Empty }, Map);
    }

    // Umbraco.Code.MapAll
    private static void Map(HealthCheckActionRequestModel source, HealthCheckAction target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.HealthCheckId = source.HealthCheck.Id;
        target.Name = source.Name;
        target.Description = source.Description;
        target.ValueRequired = source.ValueRequired;
        target.ProvidedValueValidation = source.ProvidedValueValidation;
        target.ProvidedValueValidationRegex = source.ProvidedValueValidationRegex;
        target.ProvidedValue = source.ProvidedValue;
        target.ActionParameters = source.ActionParameters;
    }

    // Umbraco.Code.MapAll
    private static void Map(HealthCheckAction source, HealthCheckActionRequestModel target, MapperContext context)
    {
        Guid healthCheckId = source.HealthCheckId
                             ?? throw new ArgumentException("Cannot map a health check action without a health check", nameof(source));

        target.HealthCheck = new ReferenceByIdModel(healthCheckId);
        target.Alias = source.Alias;
        target.Name = source.Name;
        target.Description = source.Description;
        target.ValueRequired = source.ValueRequired;
        target.ProvidedValue = source.ProvidedValue;
        target.ProvidedValueValidation = source.ProvidedValueValidation;
        target.ProvidedValueValidationRegex = source.ProvidedValueValidationRegex;
        target.ActionParameters = source.ActionParameters;
    }

    // Umbraco.Code.MapAll
    private static void Map(HealthCheckStatus source, HealthCheckResultResponseModel target, MapperContext context)
    {
        target.Message = source.Message;
        target.ResultType = source.ResultType;
        target.ReadMoreLink = source.ReadMoreLink;
        target.Actions = context.MapEnumerable<HealthCheckAction, HealthCheckActionRequestModel>(source.Actions);
    }

    // Umbraco.Code.MapAll
    private static void Map(Core.HealthChecks.HealthCheck source, HealthCheckViewModel target, MapperContext context)
    {
        target.Id = source.Id;
        target.Name = source.Name;
        target.Description = source.Description;
    }

    // Umbraco.Code.MapAll
    private static void Map(IGrouping<string?, Core.HealthChecks.HealthCheck> source, HealthCheckGroupPresentationModel target, MapperContext context)
    {
        if (source.Key is not null)
        {
            target.Name = source.Key;
        }

        target.Checks = context.MapEnumerable<Core.HealthChecks.HealthCheck, HealthCheckViewModel>(source.OrderBy(x => x.Name));
    }

    // Umbraco.Code.MapAll
    private static void Map(IGrouping<string?, Core.HealthChecks.HealthCheck> source, HealthCheckGroupResponseModel target, MapperContext context)
    {
        if (source.Key is not null)
        {
            target.Name = source.Key;
        }
    }
}
