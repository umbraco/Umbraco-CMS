using System;
using System.Linq;
using System.Xml.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// A startup handler for dealing with trees
    /// </summary>
    [Obsolete("This is no longer used, currenltly only here to register some model mappers")]
    public class ApplicationTreeRegistrar : ApplicationEventHandler, IMapperConfiguration
    {        
        /// <summary>
        /// Configures automapper model mappings
        /// </summary>
        public void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<Umbraco.Core.Models.ApplicationTree, ApplicationTree>()
                .ForMember(x => x.Silent, opt => opt.Ignore())
                .ForMember(x => x.AssemblyName, opt => opt.Ignore())
                .ForMember(x => x.Action, opt => opt.Ignore());

            config.CreateMap<ApplicationTree, Umbraco.Core.Models.ApplicationTree>();

            //.ReverseMap(); //two way
        }

    }
}
 