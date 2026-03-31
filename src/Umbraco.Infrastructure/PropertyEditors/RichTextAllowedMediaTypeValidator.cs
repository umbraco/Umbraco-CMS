// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
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
    private readonly HtmlImageSourceParser _imageSourceParser;
    private readonly IMediaService _mediaService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger _logger;
    private readonly AllowedMediaTypeHelper _allowedMediaTypeHelper;

    public RichTextAllowedMediaTypeValidator(
        HtmlImageSourceParser imageSourceParser,
        IMediaService mediaService,
        ILocalizedTextService localizedTextService,
        IJsonSerializer jsonSerializer,
        ILogger logger,
        AllowedMediaTypeHelper allowedMediaTypeHelper)
    {
        _imageSourceParser = imageSourceParser;
        _mediaService = mediaService;
        _localizedTextService = localizedTextService;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
        _allowedMediaTypeHelper = allowedMediaTypeHelper;
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

        HashSet<string> allowedTypeKeys = AllowedMediaTypeHelper.ParseAllowedTypeKeys(configuration.AllowedMediaTypes);

        // No allowed types configured = all types are allowed
        if (allowedTypeKeys.Count == 0)
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
            if (_allowedMediaTypeHelper.IsAllowed(media.ContentType.Alias, allowedTypeKeys) is false)
            {
                var message = $"{_localizedTextService.Localize("validation", "invalidMediaType")}: '{media.Name}'";
                return [new ValidationResult(message, ["value"])];
            }
        }

        return [];
    }
}
