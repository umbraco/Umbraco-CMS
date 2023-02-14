using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Plugin;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

public class AllPluginsController : PackageControllerBase
{
    private readonly IPluginConfigurationService _pluginConfigurationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllPluginsController(IPluginConfigurationService pluginConfigurationService, IUmbracoMapper umbracoMapper)
    {
        _pluginConfigurationService = pluginConfigurationService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("plugins")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<PluginConfigurationViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<PluginConfigurationViewModel>>> AllPlugins(int skip = 0, int take = 100)
    {
        PluginConfiguration[] pluginConfigurations = (await _pluginConfigurationService.GetPluginConfigurationsAsync()).ToArray();
        return Ok(
            new PagedViewModel<PluginConfigurationViewModel>
            {
                Items = _umbracoMapper.MapEnumerable<PluginConfiguration, PluginConfigurationViewModel>(pluginConfigurations.Skip(skip).Take(take)),
                Total = pluginConfigurations.Length
            });
    }
}
