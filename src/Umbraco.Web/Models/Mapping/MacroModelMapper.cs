using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for macros.
    /// </summary>
    internal class MacroModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            //FROM IMacro TO EntityBasic
            config.CreateMap<IMacro, EntityBasic>()
                  .ForMember(entityBasic => entityBasic.Icon, expression => expression.UseValue("icon-settings-alt"))
                  .ForMember(dto => dto.ParentId, expression => expression.UseValue(-1))
                  .ForMember(dto => dto.Path, expression => expression.ResolveUsing(macro => "-1," + macro.Id))
                  .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                  .ForMember(dto => dto.AdditionalData, expression => expression.Ignore());

            config.CreateMap<IMacro, IEnumerable<MacroParameter>>()
                    .ConvertUsing(macro => macro.Properties.Select(Mapper.Map<MacroParameter>).ToList());

            config.CreateMap<IMacroProperty, MacroParameter>()
                .ForMember(x => x.View, expression => expression.Ignore())
                .ForMember(x => x.Configuration, expression => expression.Ignore())
                .ForMember(x => x.Value, expression => expression.Ignore())
                .AfterMap((property, parameter) =>
                {
                    //map the view and the config

                    var paramEditor = ParameterEditorResolver.Current.GetByAlias(property.EditorAlias);
                    if (paramEditor == null)
                    {
                        //we'll just map this to a text box
                        paramEditor = ParameterEditorResolver.Current.GetByAlias(Constants.PropertyEditors.TextboxAlias);
                        LogHelper.Warn<MacroModelMapper>("Could not resolve a parameter editor with alias " + property.EditorAlias + ", a textbox will be rendered in it's place");
                    }

                    parameter.View = paramEditor.ValueEditor.View;
                    //set the config
                    parameter.Configuration = paramEditor.Configuration;

                });

        }

    }
}