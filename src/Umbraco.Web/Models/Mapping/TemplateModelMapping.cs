using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class TemplateModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<ITemplate, TemplateDisplay>();

            config.CreateMap<TemplateDisplay, Template>()
                .ForMember(x => x.Key, exp => exp.Ignore())
                .ForMember(x => x.Path, exp => exp.Ignore())
                .ForMember(x => x.CreateDate, exp => exp.Ignore())
                .ForMember(x => x.UpdateDate, exp => exp.Ignore())
                .ForMember(x => x.VirtualPath, exp => exp.Ignore())
                .ForMember(x => x.Path, exp => exp.Ignore());
        }
    }
}
