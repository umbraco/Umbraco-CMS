using AutoMapper;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.Core.Persistence.Factories
{
    /// <summary>
    /// This configures all of the AutoMapper supported mapping factories
    /// </summary>
    internal class ModelFactoryConfiguration : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config)
        {
            //TODO: Implement AutoMapper for the other factories!
            UserSectionFactory.ConfigureMappings(config);
        }
    }
}