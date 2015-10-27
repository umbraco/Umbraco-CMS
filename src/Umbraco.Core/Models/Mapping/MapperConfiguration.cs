using AutoMapper;

namespace Umbraco.Core.Models.Mapping
{
    /// <summary>
    /// Used to declare a mapper configuration
    /// </summary>
    /// <remarks>
    /// Use this class if your mapper configuration isn't also explicitly an ApplicationEventHandler.
    /// </remarks>
    public abstract class MapperConfiguration : ApplicationEventHandler, IMapperConfiguration
    {
        public abstract void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext);
    }

    
}