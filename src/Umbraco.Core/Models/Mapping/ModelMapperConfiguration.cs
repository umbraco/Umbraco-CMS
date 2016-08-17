using AutoMapper;

namespace Umbraco.Core.Models.Mapping
{
    /// <summary>
    /// Used to declare a mapper configuration
    /// </summary>    
    public abstract class ModelMapperConfiguration
    {
        public abstract void ConfigureMappings(IMapperConfiguration config, ApplicationContext applicationContext);
    }

    
}