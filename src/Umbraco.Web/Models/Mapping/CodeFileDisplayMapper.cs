using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    public class CodeFileDisplayMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IPartialView, CodeFileDisplay>()
                .ForMember(x => x.FileType, exp => exp.Ignore())
                .ForMember(x => x.Notifications, exp => exp.Ignore())
                .ForMember(x => x.Path, exp => exp.Ignore())
                .ForMember(x => x.Snippet, exp => exp.Ignore());

            config.CreateMap<Script, CodeFileDisplay>()
                .ForMember(x => x.FileType, exp => exp.Ignore())
                .ForMember(x => x.Notifications, exp => exp.Ignore())
                .ForMember(x => x.Path, exp => exp.Ignore())
                .ForMember(x => x.Snippet, exp => exp.Ignore());

            config.CreateMap<CodeFileDisplay, IPartialView>()
                .IgnoreDeletableEntityCommonProperties()
                .ForMember(x => x.Id, exp => exp.Ignore())
                .ForMember(x => x.Key, exp => exp.Ignore())
                .ForMember(x => x.Path, exp => exp.Ignore())
                .ForMember(x => x.Path, exp => exp.Ignore())
                .ForMember(x => x.Alias, exp => exp.Ignore())
                .ForMember(x => x.Name, exp => exp.Ignore())
                .ForMember(x => x.OriginalPath, exp => exp.Ignore())
                .ForMember(x => x.HasIdentity, exp => exp.Ignore());

            config.CreateMap<CodeFileDisplay, Script>()
                .IgnoreDeletableEntityCommonProperties()
                .ForMember(x => x.Id, exp => exp.Ignore())
                .ForMember(x => x.Key, exp => exp.Ignore())
                .ForMember(x => x.Path, exp => exp.Ignore())
                .ForMember(x => x.Path, exp => exp.Ignore())
                .ForMember(x => x.Alias, exp => exp.Ignore())
                .ForMember(x => x.Name, exp => exp.Ignore())
                .ForMember(x => x.OriginalPath, exp => exp.Ignore())
                .ForMember(x => x.HasIdentity, exp => exp.Ignore());
        }
    }
}
