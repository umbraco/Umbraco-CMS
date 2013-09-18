using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
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
                  .ForMember(dto => dto.Path, expression => expression.ResolveUsing(macro => "-1," + macro.Id));

            config.CreateMap<IMacro, IEnumerable<MacroParameter>>()
                    .ConvertUsing(macro => macro.Properties.Select(Mapper.Map<MacroParameter>).ToList());

            config.CreateMap<IMacroProperty, MacroParameter>();

        }

    }
}