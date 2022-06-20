using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace Umbraco.Cms.Web.BackOffice.PropertyEditors;

/// <summary>
///     ApiController to provide RTE configuration with available plugins and commands from the RTE config
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class RichTextPreValueController : UmbracoAuthorizedJsonController
{
    private readonly IOptions<RichTextEditorSettings> _richTextEditorSettings;

    public RichTextPreValueController(IOptions<RichTextEditorSettings> richTextEditorSettings) =>
        _richTextEditorSettings = richTextEditorSettings;

    public RichTextEditorConfiguration GetConfiguration()
    {
        RichTextEditorSettings? settings = _richTextEditorSettings.Value;

        var config = new RichTextEditorConfiguration
        {
            Plugins = settings.Plugins.Select(x => new RichTextEditorPlugin { Name = x }),
            Commands =
                settings.Commands.Select(x =>
                    new RichTextEditorCommand { Alias = x.Alias, Mode = x.Mode, Name = x.Name }),
            ValidElements = settings.ValidElements,
            InvalidElements = settings.InvalidElements,
            CustomConfig = settings.CustomConfig
        };

        return config;
    }
}
