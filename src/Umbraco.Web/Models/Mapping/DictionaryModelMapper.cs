namespace Umbraco.Web.Models.Mapping
{
    using AutoMapper;

    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Core.Models.Mapping;
    using Umbraco.Web.Models.ContentEditing;

    /// <summary>
    /// The dictionary model mapper.
    /// </summary>
    internal class DictionaryModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IDictionaryItem, DictionaryDisplay>()
                .ForMember(
                    x => x.Udi,
                    expression => expression.MapFrom(
                        content => Udi.Create(Constants.UdiEntityType.DictionaryItem, content.Key))).ForMember(
                    x => x.Name,
                    expression => expression.MapFrom(content => content.ItemKey));
        }
    }
}
