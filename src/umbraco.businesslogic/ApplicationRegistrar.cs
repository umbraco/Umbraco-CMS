using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using umbraco.BusinessLogic.Utils;
using umbraco.DataLayer;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.BusinessLogic
{
    public class ApplicationRegistrar : ApplicationEventHandler, IMapperConfiguration
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // Load all Applications by attribute and add them to the XML config
            var types = PluginManager.Current.ResolveApplications();

            //since applications don't populate their metadata from the attribute and because it is an interface, 
            //we need to interrogate the attributes for the data. Would be better to have a base class that contains 
            //metadata populated by the attribute. Oh well i guess.
            var attrs = types.Select(x => x.GetCustomAttributes<ApplicationAttribute>(false).Single())
                             .Where(x => applicationContext.Services.SectionService.GetByAlias(x.Alias) == null)
                             .ToArray();

            applicationContext.Services.SectionService.Initialize(attrs.Select(x => new Section(x.Name, x.Alias, x.Icon, x.SortOrder)));                
        }

        public void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<Section, Application>()
                  .ForMember(x => x.alias, expression => expression.MapFrom(x => x.Alias))
                  .ForMember(x => x.icon, expression => expression.MapFrom(x => x.Icon))
                  .ForMember(x => x.name, expression => expression.MapFrom(x => x.Name))
                  .ForMember(x => x.sortOrder, expression => expression.MapFrom(x => x.SortOrder)).ReverseMap();
            config.CreateMap<Application, Section>()
                  .ForMember(x => x.Alias, expression => expression.MapFrom(x => x.alias))
                  .ForMember(x => x.Icon, expression => expression.MapFrom(x => x.icon))
                  .ForMember(x => x.Name, expression => expression.MapFrom(x => x.name))
                  .ForMember(x => x.SortOrder, expression => expression.MapFrom(x => x.sortOrder));
        }
    }
}