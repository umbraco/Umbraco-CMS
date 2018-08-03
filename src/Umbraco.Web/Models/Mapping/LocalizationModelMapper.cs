using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <inheritdoc />
    /// <summary>
    /// The dictionary model mapper.
    /// </summary>
    internal class LocalizationModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<ILanguage, LanguageForContentTranslation>()
                .ForMember(x => x.Name, expression => expression.MapFrom(l => l.CultureInfo.DisplayName))
                .ForMember(x => x.IsoCode, expression => expression.MapFrom(l => l.IsoCode));
        }
    }
}
