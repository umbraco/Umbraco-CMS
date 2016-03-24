using AutoMapper;

namespace Umbraco.Core.Models.Mapping
{
    /// <summary>
    /// Used to declare a mapper configuration
    /// </summary>
    /// <remarks>
    /// Use this class if your mapper configuration isn't also explicitly an ApplicationEventHandler.
    /// </remarks>
    public abstract class ModelMapperConfiguration
    {
        public abstract void ConfigureMappings(IMapperConfiguration config, ApplicationContext applicationContext);
    }

    
}