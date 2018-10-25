using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for macros.
    /// </summary>
    internal class MacroMapperProfile : Profile
    {
        public MacroMapperProfile()
        {
            //FROM IMacro TO EntityBasic
            CreateMap<IMacro, EntityBasic>()
                .ForMember(x => x.Udi, expression => expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.Macro, content.Key)))
                  .ForMember(entityBasic => entityBasic.Icon, expression => expression.UseValue("icon-settings-alt"))
                  .ForMember(dto => dto.ParentId, expression => expression.UseValue(-1))
                  .ForMember(dto => dto.Path, expression => expression.ResolveUsing(macro => "-1," + macro.Id))
                  .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                  .ForMember(dto => dto.AdditionalData, expression => expression.Ignore());

            CreateMap<IMacro, IEnumerable<MacroParameter>>()
                    .ConvertUsing(macro => macro.Properties.Values.Select(Mapper.Map<MacroParameter>).ToList());

            CreateMap<IMacroProperty, MacroParameter>()
                .ForMember(x => x.View, expression => expression.Ignore())
                .ForMember(x => x.Configuration, expression => expression.Ignore())
                .ForMember(x => x.Value, expression => expression.Ignore())
                .AfterMap((property, parameter) =>
                {
                    //map the view and the config
                    // we need to show the depracated ones for backwards compatibility
                    var paramEditor = Current.ParameterEditors[property.EditorAlias]; // fixme - include/filter deprecated?!
                    if (paramEditor == null)
                    {
                        //we'll just map this to a text box
                        paramEditor = Current.ParameterEditors[Constants.PropertyEditors.Aliases.TextBox];
                        Current.Logger.Warn<MacroMapperProfile>("Could not resolve a parameter editor with alias {PropertyEditorAlias}, a textbox will be rendered in it's place", property.EditorAlias);
                    }

                    parameter.View = paramEditor.GetValueEditor().View;
                    //set the config
                    parameter.Configuration = paramEditor.DefaultConfiguration;
                });
        }
    }
}
