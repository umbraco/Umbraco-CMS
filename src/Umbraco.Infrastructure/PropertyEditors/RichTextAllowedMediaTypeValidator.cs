// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Validates that media items referenced in RTE markup are of an allowed media type.
/// </summary>
internal sealed class RichTextAllowedMediaTypeValidator : IValueValidator
{
    private const string MediaTypeCacheKeyFormat = nameof(RichTextAllowedMediaTypeValidator) + "_MediaTypeKey_{0}";

    private readonly HtmlImageSourceParser _imageSourceParser;
    private readonly IMediaService _mediaService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger _logger;
    private readonly AppCaches _appCaches;

    public RichTextAllowedMediaTypeValidator(
        HtmlImageSourceParser imageSourceParser,
        IMediaService mediaService,
        IMediaTypeService mediaTypeService,
        ILocalizedTextService localizedTextService,
        IJsonSerializer jsonSerializer,
        ILogger logger,
        AppCaches appCaches)
    {
        _imageSourceParser = imageSourceParser;
        _mediaService = mediaService;
        _mediaTypeService = mediaTypeService;
        _localizedTextService = localizedTextService;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
        _appCaches = appCaches;
    }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(
        object? value,
        string? valueType,
        object? dataTypeConfiguration,
        PropertyValidationContext validationContext)
    {
        if (dataTypeConfiguration is not RichTextConfiguration configuration)
        {
            return [];
        }

        var allowedTypes = configuration.AllowedMediaTypes?.Split(
            Constants.CharArrays.Comma,
            StringSplitOptions.RemoveEmptyEntries);

        // No allowed types configured = all types are allowed
        if (allowedTypes is null || allowedTypes.Length == 0)
        {
            return [];
        }

        if (!RichTextPropertyEditorHelper.TryParseRichTextEditorValue(value, _jsonSerializer, _logger, out RichTextEditorValue? richTextEditorValue))
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(richTextEditorValue.Markup))
        {
            return [];
        }

        IEnumerable<Udi> udis = _imageSourceParser.FindUdisFromDataAttributes(richTextEditorValue.Markup);
        Guid[] mediaKeys = udis.OfType<GuidUdi>().Select(udi => udi.Guid).Distinct().ToArray();

        if (mediaKeys.Length == 0)
        {
            return [];
        }

        IEnumerable<IMedia> mediaItems = _mediaService.GetByIds(mediaKeys);

        foreach (IMedia media in mediaItems)
        {
            var typeAlias = media.ContentType.Alias;
            var typeKey = GetMediaTypeKey(typeAlias);

            if (typeKey is null || allowedTypes.Contains(typeKey) is false)
            {
                return
                [
                    new ValidationResult(
                        _localizedTextService.Localize("validation", "invalidMediaType"),
                        ["value"])
                ];
            }
        }

        return [];
    }

    private string? GetMediaTypeKey(string typeAlias)
    {
        string? GetMediaTypeKeyFromService(string alias) => _mediaTypeService.Get(alias)?.Key.ToString();

        if (_appCaches.RequestCache.IsAvailable is false)
        {
            return GetMediaTypeKeyFromService(typeAlias);
        }

        var cacheKey = string.Format(MediaTypeCacheKeyFormat, typeAlias);
        var typeKey = _appCaches.RequestCache.GetCacheItem<string?>(cacheKey);
        if (typeKey is null)
        {
            typeKey = GetMediaTypeKeyFromService(typeAlias);
            if (typeKey is not null)
            {
                _appCaches.RequestCache.Set(cacheKey, typeKey);
            }
        }

        return typeKey;
    }
}
