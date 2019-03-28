using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class MacroMapperProfile : IMapperProfile
    {
        private readonly ParameterEditorCollection _parameterEditors;
        private readonly ILogger _logger;

        public MacroMapperProfile(ParameterEditorCollection parameterEditors, ILogger logger)
        {
            _parameterEditors = parameterEditors;
            _logger = logger;
        }

        public void DefineMaps(Mapper mapper)
        {
            mapper.Define<IMacro, EntityBasic>((source, context) => new EntityBasic(), Map);
            mapper.Define<IMacro, IEnumerable<MacroParameter>>((source, context) => source.Properties.Values.Select(context.Mapper.Map<MacroParameter>).ToList());
            mapper.Define<IMacroProperty, MacroParameter>((source, context) => new MacroParameter(), Map);
        }

        // Umbraco.Code.MapAll -Trashed -AdditionalData
        private static void Map(IMacro source, EntityBasic target, MapperContext context)
        {
            target.Alias = source.Alias;
            target.Icon = "icon-settings-alt";
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentId = -1;
            target.Path = "-1," + source.Id;
            target.Udi = Udi.Create(Constants.UdiEntityType.Macro, source.Key);
        }

        // Umbraco.Code.MapAll -Value
        private void Map(IMacroProperty source, MacroParameter target, MapperContext context)
        {
            target.Alias = source.Alias;
            target.Name = source.Name;
            target.SortOrder = source.SortOrder;

            //map the view and the config
            // we need to show the deprecated ones for backwards compatibility
            var paramEditor = _parameterEditors[source.EditorAlias]; // TODO: include/filter deprecated?!
            if (paramEditor == null)
            {
                //we'll just map this to a text box
                paramEditor = _parameterEditors[Constants.PropertyEditors.Aliases.TextBox];
                _logger.Warn<MacroMapperProfile>("Could not resolve a parameter editor with alias {PropertyEditorAlias}, a textbox will be rendered in it's place", source.EditorAlias);
            }

            target.View = paramEditor.GetValueEditor().View;

            // sets the parameter configuration to be the default configuration editor's configuration,
            // ie configurationEditor.DefaultConfigurationObject, prepared for the value editor, ie
            // after ToValueEditor - important to use DefaultConfigurationObject here, because depending
            // on editors, ToValueEditor expects the actual strongly typed configuration - not the
            // dictionary thing returned by DefaultConfiguration

            var configurationEditor = paramEditor.GetConfigurationEditor();
            target.Configuration = configurationEditor.ToValueEditor(configurationEditor.DefaultConfigurationObject);
        }
    }
}
