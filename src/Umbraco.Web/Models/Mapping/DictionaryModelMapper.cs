using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
	internal class DictionaryModelMapper : MapperConfiguration
	{
		public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
		{
			var lazyLocalizationService = new Lazy<ILocalizationService>(() => applicationContext.Services.LocalizationService);

			config.CreateMap<IDictionaryItem, DictionaryItemDisplay>()
				.ForMember(x => x.Name, expression => expression.MapFrom(definition => definition.ItemKey))
				.ForMember(x => x.ParentGuid, expression => expression.MapFrom(definition => definition.ParentId))
				.ForMember(x => x.ParentId, expression => expression.MapFrom(definition => -1))
				.ForMember(x => x.Translations, expression => expression.MapFrom(definition => definition.Translations))
				.ForMember(x => x.Notifications, expression => expression.Ignore())
				.ForMember(x => x.Icon, expression => expression.Ignore())
				.ForMember(x => x.Alias, expression => expression.Ignore());
		}
	}
}