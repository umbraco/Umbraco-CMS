using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PropertyEditors
{
    public class MultiUrlPickerValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IEntityService _entityService;
        private readonly ILogger<MultiUrlPickerValueEditor> _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IPublishedUrlProvider _publishedUrlProvider;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public MultiUrlPickerValueEditor(IEntityService entityService, IPublishedSnapshotAccessor publishedSnapshotAccessor, ILogger<MultiUrlPickerValueEditor> logger, IDataTypeService dataTypeService, ILocalizationService localizationService, ILocalizedTextService localizedTextService, IShortStringHelper shortStringHelper, DataEditorAttribute attribute, IUmbracoContextAccessor umbracoContextAccessor, IPublishedUrlProvider publishedUrlProvider)
            : base(dataTypeService, localizationService, localizedTextService, shortStringHelper, attribute)
        {
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _publishedUrlProvider = publishedUrlProvider;
        }

        public override object ToEditor(IProperty property, string culture = null, string segment = null)
        {
            var value = property.GetValue(culture, segment)?.ToString();

            if (string.IsNullOrEmpty(value))
            {
                return Enumerable.Empty<object>();
            }

            try
            {
                var links = JsonConvert.DeserializeObject<List<MultiUrlPickerValueEditor.LinkDto>>(value);

                var documentLinks = links.FindAll(link => link.Udi != null && link.Udi.EntityType == Constants.UdiEntityType.Document);
                var mediaLinks = links.FindAll(link => link.Udi != null && link.Udi.EntityType == Constants.UdiEntityType.Media);

                var entities = new List<IEntitySlim>();
                if (documentLinks.Count > 0)
                {
                    entities.AddRange(
                        _entityService.GetAll(UmbracoObjectTypes.Document, documentLinks.Select(link => link.Udi.Guid).ToArray())
                    );
                }

                if (mediaLinks.Count > 0)
                {
                    entities.AddRange(
                        _entityService.GetAll(UmbracoObjectTypes.Media, mediaLinks.Select(link => link.Udi.Guid).ToArray())
                    );
                }

                var result = new List<LinkDisplay>();
                foreach (var dto in links)
                {
                    GuidUdi udi = null;
                    var icon = "icon-link";
                    var published = true;
                    var trashed = false;
                    var url = dto.Url;

                    if (dto.Udi != null)
                    {
                        IUmbracoEntity entity = entities.Find(e => e.Key == dto.Udi.Guid);
                        if (entity == null)
                        {
                            continue;
                        }

                        if (entity is IDocumentEntitySlim documentEntity)
                        {
                            icon = documentEntity.ContentTypeIcon;
                            published = culture == null ? documentEntity.Published : documentEntity.PublishedCultures.Contains(culture);
                            udi = new GuidUdi(Constants.UdiEntityType.Document, documentEntity.Key);
                            url = _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(entity.Key)?.Url(_publishedUrlProvider) ?? "#";
                            trashed = documentEntity.Trashed;
                        }
                        else if(entity is IContentEntitySlim contentEntity)
                        {
                            icon = contentEntity.ContentTypeIcon;
                            published = !contentEntity.Trashed;
                            udi = new GuidUdi(Constants.UdiEntityType.Media, contentEntity.Key);
                            url = _publishedSnapshotAccessor.PublishedSnapshot.Media.GetById(entity.Key)?.Url(_publishedUrlProvider) ?? "#";
                            trashed = contentEntity.Trashed;
                        }
                        else
                        {
                            // Not supported
                            continue;
                        }
                    }

                    result.Add(new LinkDisplay
                    {
                        Icon = icon,
                        Name = dto.Name,
                        Target = dto.Target,
                        Trashed = trashed,
                        Published = published,
                        QueryString = dto.QueryString,
                        Udi = udi,
                        Url = url ?? ""
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting links", ex);
            }

            return base.ToEditor(property, culture, segment);
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
                return JsonConvert.SerializeObject(
                    from link in JsonConvert.DeserializeObject<List<LinkDisplay>>(value)
                    select new MultiUrlPickerValueEditor.LinkDto
                    {
                        Name = link.Name,
                        QueryString = link.QueryString,
                        Target = link.Target,
                        Udi = link.Udi,
                        Url = link.Udi == null ? link.Url : null, // only save the URL for external links
                    },
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error saving links", ex);
            }

            return base.FromEditor(editorValue, currentValue);
        }

        [DataContract]
        public class LinkDto
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
            var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

            if (string.IsNullOrEmpty(asString)) yield break;

            var links = JsonConvert.DeserializeObject<List<LinkDto>>(asString);
            foreach (var link in links)
            {
                if (link.Udi != null) // Links can be absolute links without a Udi
                {
                    yield return new UmbracoEntityReference(link.Udi);
                }

            }
        }
    }
}
