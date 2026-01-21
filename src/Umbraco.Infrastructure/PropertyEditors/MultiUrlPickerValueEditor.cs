// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public class MultiUrlPickerValueEditor : DataValueEditor, IDataValueReference, ICacheReferencedEntities
{
    private readonly ILogger<MultiUrlPickerValueEditor> _logger;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly AppCaches _appCaches;

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public MultiUrlPickerValueEditor(
        ILogger<MultiUrlPickerValueEditor> logger,
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        DataEditorAttribute attribute,
        IPublishedUrlProvider publishedUrlProvider,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        IContentService contentService,
        IMediaService mediaService)
        : this(
            logger,
            localizedTextService,
            shortStringHelper,
            attribute,
            publishedUrlProvider,
            jsonSerializer,
            ioHelper,
            contentService,
            mediaService,
            StaticServiceProvider.Instance.GetRequiredService<AppCaches>())
    {
    }

    public MultiUrlPickerValueEditor(
        ILogger<MultiUrlPickerValueEditor> logger,
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        DataEditorAttribute attribute,
        IPublishedUrlProvider publishedUrlProvider,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        IContentService contentService,
        IMediaService mediaService,
        AppCaches appCaches)
        : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _logger = logger;
        _publishedUrlProvider = publishedUrlProvider;
        _jsonSerializer = jsonSerializer;
        _contentService = contentService;
        _mediaService = mediaService;
        _appCaches = appCaches;

        Validators.Add(new TypedJsonValidatorRunner<LinkDisplay[], MultiUrlPickerConfiguration>(
            _jsonSerializer,
            new MinMaxValidator(localizedTextService)));
    }

    /// <inheritdoc/>
    public void CacheReferencedEntities(IEnumerable<object> values)
    {
        var dtos = values
            .Select(value =>
            {
                var asString = value is string str ? str : value.ToString();
                if (string.IsNullOrEmpty(asString))
                {
                    return null;
                }

                return _jsonSerializer.Deserialize<List<LinkDto>>(asString);
            })
            .WhereNotNull()
            .SelectMany(x => x)
            .Where(x => x.Type == Constants.UdiEntityType.Document || x.Type == Constants.UdiEntityType.Media)
            .ToList();

        IList<Guid> contentKeys = GetKeys(Constants.UdiEntityType.Document, dtos);
        IList<Guid> mediaKeys = GetKeys(Constants.UdiEntityType.Media, dtos);

        if (contentKeys.Count > 0)
        {
            IEnumerable<IContent> contentItems = _contentService.GetByIds(contentKeys);
            foreach (IContent content in contentItems)
            {
                CacheContentById(content, _appCaches.RequestCache);
            }
        }

        if (mediaKeys.Count > 0)
        {
            IEnumerable<IMedia> mediaItems = _mediaService.GetByIds(mediaKeys);
            foreach (IMedia media in mediaItems)
            {
                CacheMediaById(media, _appCaches.RequestCache);
            }
        }
    }

    private IList<Guid> GetKeys(string entityType, IEnumerable<LinkDto> dtos) =>
        dtos
            .Where(x => x.Type == entityType)
            .Select(x => x.Unique ?? (x.Udi is not null ? x.Udi.Guid : Guid.Empty))
            .Where(x => x != Guid.Empty)
            .Distinct()
            .Where(x => IsAlreadyCached(x, entityType) is false)
            .ToList();

    private bool IsAlreadyCached(Guid key, string entityType) => entityType switch
    {
        Constants.UdiEntityType.Document => IsContentAlreadyCached(key, _appCaches.RequestCache),
        Constants.UdiEntityType.Media => IsMediaAlreadyCached(key, _appCaches.RequestCache),
        _ => false,
    };

    public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
    {
        var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

        if (string.IsNullOrEmpty(asString))
        {
            yield break;
        }

        List<LinkDto>? links = _jsonSerializer.Deserialize<List<LinkDto>>(asString);
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
            List<LinkDto>? links = _jsonSerializer.Deserialize<List<LinkDto>>(value);

            var result = new List<LinkDisplay>();
            if (links is null)
            {
                return result;
            }

            foreach (LinkDto dto in links)
            {
                GuidUdi? udi = dto.Udi;
                var icon = "icon-link";
                var published = true;
                var trashed = false;
                var url = dto.Url;

                if (dto.Udi != null)
                {
                    if (dto.Udi.EntityType == Constants.UdiEntityType.Document)
                    {
                        url = _publishedUrlProvider.GetUrl(dto.Udi.Guid, UrlMode.Relative, dto.Culture ?? culture);
                        IContent? c = GetAndCacheContentById(dto.Udi.Guid, _appCaches.RequestCache, _contentService);

                        if (c is not null)
                        {
                            published = culture == null
                                ? c.Published
                                : c.PublishedCultures.Contains(culture);
                            icon = c.ContentType.Icon;
                            trashed = c.Trashed;
                        }
                    }
                    else if (dto.Udi.EntityType == Constants.UdiEntityType.Media)
                    {
                        url = _publishedUrlProvider.GetMediaUrl(dto.Udi.Guid, UrlMode.Relative, culture);
                        IMedia? m = GetAndCacheMediaById(dto.Udi.Guid, _appCaches.RequestCache, _mediaService);
                        if (m is not null)
                        {
                            published = m.Trashed is false;
                            icon = m.ContentType.Icon;
                            trashed = m.Trashed;
                        }
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
                    Type = dto.Udi is null ? LinkDisplay.Types.External
                    : dto.Udi.EntityType,
                    Unique = dto.Udi?.Guid,
                    Url = url ?? string.Empty,
                    Culture = dto.Culture ?? null
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
        // the editor value is a JsonArray, which produces deserialize-able JSON with ToString() (deserialization happens later on)
        var value = editorValue.Value?.ToString();

        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        try
        {
            List<LinkDisplay>? links = _jsonSerializer.Deserialize<List<LinkDisplay>>(value);
            if (links == null || links.Count == 0)
            {
                return null;
            }

            return _jsonSerializer.Serialize(
                links.Select(link => new LinkDto
                {
                    Name = link.Name,
                    QueryString = link.QueryString,
                    Target = link.Target,
                    Udi = TypeIsUdiBased(link) ? new GuidUdi(link.Type!, link.Unique!.Value) : null,
                    Url = TypeIsExternal(link) ? link.Url : null, // only save the URL for external links
                    Culture = link.Culture ?? null,
                }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving links");
        }

        return base.FromEditor(editorValue, currentValue);
    }

    private static bool TypeIsExternal(LinkDisplay link) =>
        link.Type is not null && link.Type.Equals(LinkDisplay.Types.External, StringComparison.InvariantCultureIgnoreCase);

    private static bool TypeIsUdiBased(LinkDisplay link) =>
        link.Type is not null && link.Unique is not null &&
        (link.Type.Equals(LinkDisplay.Types.Document, StringComparison.InvariantCultureIgnoreCase)
         || link.Type.Equals(LinkDisplay.Types.Media, StringComparison.InvariantCultureIgnoreCase));

    [DataContract]
    public class LinkDto
    {
        [DataMember(Name = "name")]
        public string? Name { get; set; }

        [DataMember(Name = "target")]
        public string? Target { get; set; }

        [DataMember(Name = "unique")]
        public Guid? Unique { get; set; }

        [DataMember(Name = "type")]
        public string? Type { get; set; }

        [DataMember(Name = "udi")]
        public GuidUdi? Udi { get; set; }

        [DataMember(Name = "url")]
        public string? Url { get; set; }

        [DataMember(Name = "queryString")]
        public string? QueryString { get; set; }

        [DataMember(Name = "culture")]
        public string? Culture { get; set; }
    }

    internal sealed class MinMaxValidator : ITypedJsonValidator<LinkDisplay[], MultiUrlPickerConfiguration>
    {
        private readonly ILocalizedTextService _localizedTextService;

        public MinMaxValidator(ILocalizedTextService localizedTextService) => _localizedTextService = localizedTextService;

        public IEnumerable<ValidationResult> Validate(
            LinkDisplay[]? linksDtos,
            MultiUrlPickerConfiguration? multiUrlPickerConfiguration,
            string? valueType,
            PropertyValidationContext validationContext)
        {
           if (multiUrlPickerConfiguration is null || (linksDtos is null && multiUrlPickerConfiguration.MinNumber == 0))
           {
               return [];
           }

           if (linksDtos is null || linksDtos.Length < multiUrlPickerConfiguration.MinNumber)
           {
               return [new ValidationResult(
                   _localizedTextService.Localize(
                       "validation",
                       "entriesShort",
                       [multiUrlPickerConfiguration.MinNumber.ToString(), (multiUrlPickerConfiguration.MinNumber - (linksDtos?.Length ?? 0)).ToString()]),
                   ["value"])];
           }

           if (linksDtos.Length > multiUrlPickerConfiguration.MaxNumber && multiUrlPickerConfiguration.MaxNumber > 0)
           {
               return
               [
                   new ValidationResult(
                       _localizedTextService.Localize(
                           "validation",
                           "entriesExceed",
                           [
                               multiUrlPickerConfiguration.MaxNumber.ToString(),
                               (linksDtos.Length - multiUrlPickerConfiguration.MaxNumber).ToString()
                           ]),
                       ["value"])
               ];
           }

           return [];
        }
    }
}
