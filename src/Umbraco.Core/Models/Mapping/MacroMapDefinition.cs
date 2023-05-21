using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

public class MacroMapDefinition : IMapDefinition
{
    private readonly ILogger<MacroMapDefinition> _logger;
    private readonly ParameterEditorCollection _parameterEditors;

    public MacroMapDefinition(ParameterEditorCollection parameterEditors, ILogger<MacroMapDefinition> logger)
    {
        _parameterEditors = parameterEditors;
        _logger = logger;
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IMacro, EntityBasic>((source, context) => new EntityBasic(), Map);
        mapper.Define<IMacro, MacroDisplay>((source, context) => new MacroDisplay(), Map);
        mapper.Define<IMacro, IEnumerable<MacroParameter>>((source, context) =>
            context.MapEnumerable<IMacroProperty, MacroParameter>(source.Properties.Values).WhereNotNull());
        mapper.Define<IMacroProperty, MacroParameter>((source, context) => new MacroParameter(), Map);
    }

    // Umbraco.Code.MapAll -Trashed -AdditionalData
    private static void Map(IMacro source, EntityBasic target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Icon = Constants.Icons.Macro;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = -1;
        target.Path = "-1," + source.Id;
        target.Udi = Udi.Create(Constants.UdiEntityType.Macro, source.Key);
    }

    private void Map(IMacro source, MacroDisplay target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Icon = Constants.Icons.Macro;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = -1;
        target.Path = "-1," + source.Id;
        target.Udi = Udi.Create(Constants.UdiEntityType.Macro, source.Key);
        target.CacheByPage = source.CacheByPage;
        target.CacheByUser = source.CacheByMember;
        target.CachePeriod = source.CacheDuration;
        target.UseInEditor = source.UseInEditor;
        target.RenderInEditor = !source.DontRender;
        target.View = source.MacroSource;
    }

    // Umbraco.Code.MapAll -Value
    private void Map(IMacroProperty source, MacroParameter target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Name = source.Name;
        target.SortOrder = source.SortOrder;

        // map the view and the config
        // we need to show the deprecated ones for backwards compatibility
        IDataEditor? paramEditor = _parameterEditors[source.EditorAlias]; // TODO: include/filter deprecated?!
        if (paramEditor == null)
        {
            // we'll just map this to a text box
            paramEditor = _parameterEditors[Constants.PropertyEditors.Aliases.TextBox];
            _logger.LogWarning(
                "Could not resolve a parameter editor with alias {PropertyEditorAlias}, a textbox will be rendered in it's place",
                source.EditorAlias);
        }

        target.View = paramEditor?.GetValueEditor().View;

        // sets the parameter configuration to be the default configuration editor's configuration,
        // ie configurationEditor.DefaultConfigurationObject, prepared for the value editor, ie
        // after ToValueEditor - important to use DefaultConfigurationObject here, because depending
        // on editors, ToValueEditor expects the actual strongly typed configuration - not the
        // dictionary thing returned by DefaultConfiguration
        IConfigurationEditor? configurationEditor = paramEditor?.GetConfigurationEditor();
        target.Configuration = configurationEditor?.ToValueEditor(configurationEditor.DefaultConfigurationObject);
    }
}
