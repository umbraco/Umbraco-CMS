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

/// <summary>
/// Provides a value editor implementation for the Multi URL Picker property editor in Umbraco.
/// Handles the editing and processing of multiple URL values selected by users.
/// </summary>
public class MultiUrlPickerValueEditor : DataValueEditor, IDataValueReference, ICacheReferencedEntities
{
    private readonly ILogger<MultiUrlPickerValueEditor> _logger;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly AppCaches _appCaches;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiUrlPickerValueEditor"/> class with the specified dependencies.
    /// </summary>
    /// <param name="logger">The logger used for logging events and errors.</param>
    /// <param name="localizedTextService">The service for retrieving localized text.</param>
    /// <param name="shortStringHelper">Helper for handling short string operations.</param>
    /// <param name="attribute">The data editor attribute that describes the editor.</param>
    /// <param name="publishedUrlProvider">Provider for published URLs.</param>
    /// <param name="jsonSerializer">The serializer used for JSON operations.</param>
    /// <param name="ioHelper">Helper for IO operations.</param>
    /// <param name="contentService">Service for managing content items.</param>
    /// <param name="mediaService">Service for managing media items.</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiUrlPickerValueEditor"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{MultiUrlPickerValueEditor}"/> used for logging.</param>
    /// <param name="localizedTextService">The <see cref="ILocalizedTextService"/> for localized text operations.</param>
    /// <param name="shortStringHelper">The <see cref="IShortStringHelper"/> for string manipulation.</param>
    /// <param name="attribute">The <see cref="DataEditorAttribute"/> describing the data editor.</param>
    /// <param name="publishedUrlProvider">The <see cref="IPublishedUrlProvider"/> for resolving published URLs.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> for JSON serialization.</param>
    /// <param name="ioHelper">The <see cref="IIOHelper"/> for IO operations.</param>
    /// <param name="contentService">The <see cref="IContentService"/> for content management.</param>
    /// <param name="mediaService">The <see cref="IMediaService"/> for media management.</param>
    /// <param name="appCaches">The <see cref="AppCaches"/> instance for caching.</param>
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

    /// <summary>
    /// Extracts entity references from the provided value, which is expected to be a JSON-serialized list of links.
    /// </summary>
    /// <param name="value">The value to extract references from. This should be a JSON string representing a collection of links, or <c>null</c>.</param>
    /// <returns>
    /// An enumerable of <see cref="UmbracoEntityReference"/> objects extracted from the value. If the value is <c>null</c>, empty, or contains no valid references, an empty enumerable is returned.
    /// </returns>
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

    /// <summary>
    /// Converts the property value of a multi-URL picker to an object suitable for use in the editor UI.
    /// </summary>
    /// <param name="property">The property whose value will be converted for the editor.</param>
    /// <param name="culture">The culture to use for value conversion, or <c>null</c> for the default culture.</param>
    /// <param name="segment">The segment to use for value conversion, or <c>null</c> if not applicable.</param>
    /// <returns>
    /// A collection of <see cref="LinkDisplay"/> objects representing the editor value, or an empty collection if the property value is null or empty.
    /// If deserialization fails, the base implementation's result is returned.
    /// </returns>
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
                        url =  _publishedUrlProvider.GetUrl(dto.Udi.Guid, UrlMode.Relative, culture);
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

    /// <summary>
    /// Converts the value received from the editor (as part of the content property data) into a serialized JSON format suitable for storage.
    /// </summary>
    /// <param name="editorValue">The content property data containing the value from the editor, expected to be a JSON array of link objects.</param>
    /// <param name="currentValue">The current stored value of the property, if any.</param>
    /// <returns>
    /// A JSON string representing the collection of links if valid links are found; otherwise, <c>null</c> if the input is empty or invalid.
    /// </returns>
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

    /// <summary>
    /// Represents a data transfer object (DTO) for a link item used by the MultiUrlPicker value editor.
    /// This class typically contains information such as the URL, name, and target for each link.
    /// </summary>
    [DataContract]
    public class LinkDto
    {
        /// <summary>
        /// Gets or sets the name of the link.
        /// </summary>
        [DataMember(Name = "name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the target attribute that specifies where to open the linked document, such as "_blank" for a new tab or window.
        /// </summary>
        [DataMember(Name = "target")]
        public string? Target { get; set; }

        /// <summary>Gets or sets the unique identifier for the link.</summary>
        [DataMember(Name = "unique")]
        public Guid? Unique { get; set; }

        /// <summary>
        /// Gets or sets the type of the link, such as 'internal', 'external', or 'media'.
        /// </summary>
        [DataMember(Name = "type")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the unique document identifier (UDI) associated with this link, if available.
        /// </summary>
        [DataMember(Name = "udi")]
        public GuidUdi? Udi { get; set; }

        /// <summary>
        /// Gets or sets the absolute or relative URL associated with the link.
        /// </summary>
        [DataMember(Name = "url")]
        public string? Url { get; set; }

        /// <summary>Gets or sets the query string part of the link.</summary>
        [DataMember(Name = "queryString")]
        public string? QueryString { get; set; }
    }

    internal sealed class MinMaxValidator : ITypedJsonValidator<LinkDisplay[], MultiUrlPickerConfiguration>
    {
        private readonly ILocalizedTextService _localizedTextService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxValidator"/> class with the specified localized text service.
        /// </summary>
        /// <param name="localizedTextService">The service used to provide localized text for validation messages.</param>
        public MinMaxValidator(ILocalizedTextService localizedTextService) => _localizedTextService = localizedTextService;

        /// <summary>
        /// Validates that the number of links provided meets the minimum and maximum constraints specified in the configuration.
        /// </summary>
        /// <param name="linksDtos">An array of <see cref="LinkDisplay"/> objects representing the links to validate. May be <c>null</c>.</param>
        /// <param name="multiUrlPickerConfiguration">The configuration object specifying minimum and maximum allowed links. May be <c>null</c>.</param>
        /// <param name="valueType">The type of value being validated (not used in this implementation).</param>
        /// <param name="validationContext">The context for property validation.</param>
        /// <returns>
        /// An <see cref="IEnumerable{ValidationResult}"/> containing validation errors if the number of links is less than the minimum or greater than the maximum allowed; otherwise, an empty enumerable.
        /// </returns>
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
