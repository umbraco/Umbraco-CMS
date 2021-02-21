using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors
{
    public class MultiUrlPickerValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IEntityService _entityService;
        private readonly ILogger _logger;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public MultiUrlPickerValueEditor(IEntityService entityService, IPublishedSnapshotAccessor publishedSnapshotAccessor, ILogger logger, DataEditorAttribute attribute)
            : base(attribute)
        {
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
        {
            var value = property.GetValue(culture, segment)?.ToString();

            if (string.IsNullOrEmpty(value))
            {
                return Enumerable.Empty<object>();
            }

            try
            {
                var dtos = JsonConvert.DeserializeObject<List<LinkDto>>(value);

                // Get all entities per type (as it's faster than querying single entities)
                var entities = new List<IEntitySlim>();
                foreach (var entityTypeKeys in dtos.Where(dto => dto.Udi != null).ToLookup(dto => dto.Udi.EntityType, dto => dto.Udi.Guid))
                {
                    var objectType = Constants.UdiEntityType.ToUmbracoObjectType(entityTypeKeys.Key);
                    entities.AddRange(_entityService.GetAll(objectType, entityTypeKeys.ToArray()));
                }

                // Process links
                var result = new List<LinkDisplay>();
                foreach (var dto in dtos)
                {
                    var icon = "icon-link";
                    var trashed = false;
                    var published = true;
                    var url = dto.Url;

                    if (dto.Udi != null)
                    {
                        var entity = entities.Find(e => e.Key == dto.Udi.Guid);
                        if (entity == null)
                        {
                            // Skip unavailable content
                            continue;
                        }

                        trashed = entity.Trashed;
                        published = !trashed;

                        if (entity is IContentEntitySlim contentEntity)
                        {
                            icon = contentEntity.ContentTypeIcon;
                        }

                        if (entity is IDocumentEntitySlim documentEntity)
                        {
                            published = culture == null || !documentEntity.Variations.VariesByCulture() ? documentEntity.Published : documentEntity.PublishedCultures.Contains(culture);
                        }

                        IPublishedContent content = null;
                        switch (dto.Udi.EntityType)
                        {
                            case Constants.UdiEntityType.Document:
                                content = _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(entity.Key);
                                break;
                            case Constants.UdiEntityType.Media:
                                content = _publishedSnapshotAccessor.PublishedSnapshot.Media.GetById(entity.Key);
                                break;
                            default:
                                // Skip unsupported entity types
                                continue;
                        }

                        url = content?.Url(culture);
                    }

                    result.Add(new LinkDisplay
                    {
                        Icon = icon,
                        Name = dto.Name,
                        Target = dto.Target,
                        Trashed = trashed,
                        Published = published,
                        QueryString = dto.QueryString,
                        Udi = dto.Udi,
                        Url = url
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error<MultiUrlPickerValueEditor>("Error getting links", ex);
            }

            return base.ToEditor(property, dataTypeService, culture, segment);
        }

        public override object FromEditor(ContentPropertyData editorValue, object currentValue)
        {
            var value = editorValue.Value?.ToString();

            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            try
            {
                // Convert display to DTO
                return JsonConvert.SerializeObject(
                    JsonConvert.DeserializeObject<List<LinkDisplay>>(value).Select(link => new LinkDto
                    {
                        Name = link.Name,
                        Target = link.Target,
                        QueryString = link.QueryString,
                        Udi = link.Udi,
                        Url = link.Udi == null ? link.Url : null // Only save the URL for external links
                    }),
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
            }
            catch (Exception ex)
            {
                _logger.Error<MultiUrlPickerValueEditor>("Error saving links", ex);
            }

            return base.FromEditor(editorValue, currentValue);
        }

        [DataContract]
        internal class LinkDto
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "target")]
            public string Target { get; set; }

            [DataMember(Name = "udi")]
            public GuidUdi Udi { get; set; }

            [DataMember(Name = "url")]
            public string Url { get; set; }

            [DataMember(Name = "queryString")]
            public string QueryString { get; set; }
        }

        public IEnumerable<UmbracoEntityReference> GetReferences(object value)
        {
            var jsonValue = value?.ToString();

            if (string.IsNullOrEmpty(jsonValue)) yield break;

            var dtos = JsonConvert.DeserializeObject<List<LinkDto>>(jsonValue);
            foreach (var dto in dtos.Where(dto => dto.Udi != null))
            {
                yield return new UmbracoEntityReference(dto.Udi);
            }
        }
    }
}
