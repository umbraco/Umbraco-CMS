namespace Umbraco.Web.Models.Mapping
{
    using System;
    using System.Collections.Generic;

    using AutoMapper;

    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Core.Models.Mapping;
    using Umbraco.Core.Services;
    using Umbraco.Web.Models.ContentEditing;

    /// <summary>
    /// The dictionary model mapper.
    /// </summary>
    internal class DictionaryModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            var lazyDictionaryService = new Lazy<ILocalizationService>(() => applicationContext.Services.LocalizationService);

            config.CreateMap<IDictionaryItem, DictionaryDisplay>()
                .ForMember(
                    x => x.Udi,
                    expression => expression.MapFrom(
                        content => Udi.Create(Constants.UdiEntityType.DictionaryItem, content.Key))).ForMember(
                    x => x.Name,
                    expression => expression.MapFrom(content => content.ItemKey))
                .AfterMap(
                    (src, dest) =>
                        {
                            // build up the path to make it possible to set active item in tree
                            // TODO check if there is a better way
                            if (src.ParentId.HasValue)
                            {
                                var ids = new List<int>();

                                ids.Add(-1);

                                this.GetParentId(src.ParentId.Value, lazyDictionaryService.Value, ids);

                                ids.Add(src.Id);

                                dest.Path = string.Join(",", ids);
                            }
                            else
                            {
                                dest.Path = "-1," + src.Id;
                            }
                        });
                
        }

        /// <summary>
        /// Goes up the dictoinary tree to get all parent ids
        /// </summary>
        /// <param name="parentId">
        /// The parent id.
        /// </param>
        /// <param name="localizationService">
        /// The localization service.
        /// </param>
        /// <param name="ids">
        /// The ids.
        /// </param>
        private void GetParentId(Guid parentId, ILocalizationService localizationService, List<int> ids)
        {
            var dictionary = localizationService.GetDictionaryItemById(parentId);

            if (dictionary != null)
            {
                ids.Add(dictionary.Id);

                if (dictionary.ParentId.HasValue)
                {
                    this.GetParentId(dictionary.ParentId.Value, localizationService, ids);
                }
            }
        }
    }
}
