// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public class MultiUrlPickerValueEditor : DataValueEditor, IDataValueReference
{
    private static readonly JsonSerializerSettings _linkDisplayJsonSerializerSettings = new()
    {
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
    };

    private readonly IEntityService _entityService;
    private readonly ILogger<MultiUrlPickerValueEditor> _logger;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public MultiUrlPickerValueEditor(
        IEntityService entityService,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        ILogger<MultiUrlPickerValueEditor> logger,
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        DataEditorAttribute attribute,
        IPublishedUrlProvider publishedUrlProvider,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper)
        : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                     throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishedUrlProvider = publishedUrlProvider;
    }

    public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
    {
        var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

        if (string.IsNullOrEmpty(asString))
        {
            yield break;
        }

        List<LinkDto>? links = JsonConvert.DeserializeObject<List<LinkDto>>(asString);
        if (links is not null)
        {
            foreach (LinkDto link in links)
            {
                // Links can be absolute links without a Udi
                if (link.Udi != null)
                {
                    yield return new UmbracoEntityReference(link.Udi);
                }
            }
        }
    }

    public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
    {
        var value = property.GetValue(culture, segment)?.ToString();

        if (string.IsNullOrEmpty(value))
        {
            return Enumerable.Empty<object>();
        }

        try
        {
            List<LinkDto>? links = JsonConvert.DeserializeObject<List<LinkDto>>(value);

            List<LinkDto>? documentLinks = links?.FindAll(link =>
                link.Udi != null && link.Udi.EntityType == Constants.UdiEntityType.Document);
            List<LinkDto>? mediaLinks = links?.FindAll(link =>
                link.Udi != null && link.Udi.EntityType == Constants.UdiEntityType.Media);

            var entities = new List<IEntitySlim>();
            if (documentLinks?.Count > 0)
            {
                entities.AddRange(
                    _entityService.GetAll(
                        UmbracoObjectTypes.Document,
                        documentLinks.Select(link => link.Udi!.Guid).ToArray()));
            }

            if (mediaLinks?.Count > 0)
            {
                entities.AddRange(
                    _entityService.GetAll(UmbracoObjectTypes.Media, mediaLinks.Select(link => link.Udi!.Guid).ToArray()));
            }

            var result = new List<LinkDisplay>();
            if (links is null)
            {
                return result;
            }

            foreach (LinkDto dto in links)
            {
                GuidUdi? udi = null;
                var icon = "icon-link";
                var published = true;
                var trashed = false;
                var url = dto.Url;

                if (dto.Udi != null)
                {
                    IUmbracoEntity? entity = entities.Find(e => e.Key == dto.Udi.Guid);
                    if (entity == null)
                    {
                        continue;
                    }

                    IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
                    if (entity is IDocumentEntitySlim documentEntity)
                    {
                        icon = documentEntity.ContentTypeIcon;
                        published = culture == null
                            ? documentEntity.Published
                            : documentEntity.PublishedCultures.Contains(culture);
                        udi = new GuidUdi(Constants.UdiEntityType.Document, documentEntity.Key);
                        url = publishedSnapshot.Content?.GetById(entity.Key)?.Url(_publishedUrlProvider) ?? "#";
                        trashed = documentEntity.Trashed;
                    }
                    else if (entity is IContentEntitySlim contentEntity)
                    {
                        icon = contentEntity.ContentTypeIcon;
                        published = !contentEntity.Trashed;
                        udi = new GuidUdi(Constants.UdiEntityType.Media, contentEntity.Key);
                        url = publishedSnapshot.Media?.GetById(entity.Key)?.Url(_publishedUrlProvider) ?? "#";
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
                    Url = url ?? string.Empty,
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting links");
        }

        return base.ToEditor(property, culture, segment);
    }

    public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
    {
        var value = editorValue.Value?.ToString();

        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        try
        {
            List<LinkDisplay>? links = JsonConvert.DeserializeObject<List<LinkDisplay>>(value);
            if (links?.Count == 0)
            {
                return null;
            }

            return JsonConvert.SerializeObject(
                from link in links
                select new LinkDto
                {
                    Name = link.Name,
                    QueryString = link.QueryString,
                    Target = link.Target,
                    Udi = link.Udi,
                    Url = link.Udi == null ? link.Url : null, // only save the URL for external links
                },
                _linkDisplayJsonSerializerSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving links");
        }

        return base.FromEditor(editorValue, currentValue);
    }

    [DataContract]
    public class LinkDto
    {
        [DataMember(Name = "name")]
        public string? Name { get; set; }

        [DataMember(Name = "target")]
        public string? Target { get; set; }

        [DataMember(Name = "udi")]
        public GuidUdi? Udi { get; set; }

        [DataMember(Name = "url")]
        public string? Url { get; set; }

        [DataMember(Name = "queryString")]
        public string? QueryString { get; set; }
    }
}
