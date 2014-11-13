using System;
using System.Collections;
using System.Collections.Generic;
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
    /// <summary>
    /// A startup handler for putting the app config in the config file based on attributes found
    /// </summary>
    /// /// <remarks>
    /// TODO: This is really not a very ideal process but the code is found here because tree plugins are in the Web project or the legacy business logic project.
    /// Moving forward we can put the base tree plugin classes in the core and then this can all just be taken care of normally within the service.
    /// </remarks>
    public class ApplicationRegistrar : ApplicationEventHandler, IMapperConfiguration
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //initialize the section service with a lazy collection of found app plugins
            applicationContext.Services.SectionService.Initialize(new LazyEnumerableSections());                
        }

        /// <summary>
        /// Configure a few auto-mappings
        /// </summary>
        /// <param name="config"></param>
        /// <param name="applicationContext"></param>
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

        /// <summary>
        /// This class is here so that we can provide lazy access to tree scanning for when it is needed
        /// </summary>
        private class LazyEnumerableSections : IEnumerable<Section>
        {
            public LazyEnumerableSections()
            {
                _lazySections = new Lazy<IEnumerable<Section>>(() =>
                {
                    // Load all Applications by attribute and add them to the XML config
                    var types = PluginManager.Current.ResolveApplications();

                    //since applications don't populate their metadata from the attribute and because it is an interface, 
                    //we need to interrogate the attributes for the data. Would be better to have a base class that contains 
                    //metadata populated by the attribute. Oh well i guess.
                    var attrs = types.Select(x => x.GetCustomAttributes<ApplicationAttribute>(false).Single());                                   
                    return attrs.Select(x => new Section(x.Name, x.Alias, x.Icon, x.SortOrder)).ToArray();
                });
            }

            private readonly Lazy<IEnumerable<Section>> _lazySections;

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<Section> GetEnumerator()
            {
                return _lazySections.Value.GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}