using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Umbraco.Core.Models.Mapping
{
    /// <summary>
    /// This is used to explicitly decorate any ApplicationEventHandler class if there are
    /// AutoMapper configurations defined. 
    /// </summary>
    /// <remarks>
    /// All automapper configurations are done during startup
    /// inside an Automapper Initialize call which is better for performance
    /// </remarks>
    public interface IMapperConfiguration : IApplicationEventHandler
    {
        void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext);
    }
}
