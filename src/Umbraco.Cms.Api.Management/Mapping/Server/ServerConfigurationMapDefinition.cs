using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Server;

public class ServerConfigurationMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IDictionary<string, string>, ServerTroubleshootingResponseModel>((_, _) => new ServerTroubleshootingResponseModel { Items = Array.Empty<ServerConfigurationItemResponseModel>() }, Map);
        mapper.Define<ServerInformation, ServerInformationResponseModel>((_, _) => new ServerInformationResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IDictionary<string, string> source, ServerTroubleshootingResponseModel target, MapperContext context)
        => target.Items = source.Select(kvp => new ServerConfigurationItemResponseModel
        {
            Name = kvp.Key,
            Data = kvp.Value
        });

    // Umbraco.Code.MapAll
    private void Map(ServerInformation source, ServerInformationResponseModel target, MapperContext context)
    {
        target.RuntimeMode = source.RuntimeMode;
        target.Version = source.SemVersion.ToSemanticString();
        target.AssemblyVersion = source.SemVersion.ToSemanticStringWithoutBuild();
        target.BaseUtcOffset = source.TimeZoneInfo.BaseUtcOffset.ToString();
    }
}
