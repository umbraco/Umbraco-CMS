using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Element picker property editor that stores element keys
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ElementPicker,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class ElementPickerPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    public ElementPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ElementPickerConfigurationEditor(_ioHelper);

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<ElementPickerPropertyValueEditor>(Attribute!);

    internal sealed class ElementPickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IJsonSerializer _jsonSerializer;

        public ElementPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService,
            IElementService elementService,
            ICoreScopeProvider coreScopeProvider)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
            Validators.Add(new TypedValidatorRunner<List<string>, ElementPickerConfiguration>(
                new MinMaxValidator(localizedTextService),
                new AllowedTypeValidator(localizedTextService, elementService, coreScopeProvider)));
        }

        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            var asString = value as string ?? value?.ToString();
            if (string.IsNullOrEmpty(asString))
            {
                yield break;
            }

            IEnumerable<Guid>? elementIds = _jsonSerializer.Deserialize<IEnumerable<Guid>>(asString);
            if (elementIds is null)
            {
                yield break;
            }

            foreach (Guid elementId in elementIds)
            {
                yield return new UmbracoEntityReference(Udi.Create(Constants.UdiEntityType.Element, elementId));
            }
        }
    }

    /// <summary>
    /// Validator to ensure that the number of selected elements is within the configured min/max limits, if any.
    /// </summary>
    internal sealed class MinMaxValidator : ITypedJsonValidator<List<string>, ElementPickerConfiguration>
    {
        private readonly ILocalizedTextService _localizedTextService;

        public MinMaxValidator(ILocalizedTextService localizedTextService)
        {
            _localizedTextService = localizedTextService;
        }

        public IEnumerable<ValidationResult> Validate(
            List<string>? value,
            ElementPickerConfiguration? configuration,
            string? valueType,
            PropertyValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (configuration is null || configuration.ValidationLimit is null)
            {
                return validationResults;
            }

            if (configuration.ValidationLimit.Min > 0 && (value is null || value.Count < configuration.ValidationLimit.Min))
            {
                validationResults.Add(new ValidationResult(
                    _localizedTextService.Localize(
                        "validation",
                        "entriesShort",
                        [configuration.ValidationLimit.Min.ToString(), (configuration.ValidationLimit.Min - (value?.Count ?? 0)).ToString()
                        ]),
                    ["value"]));
            }

            if (value is null)
            {
                return validationResults;
            }

            if (configuration.ValidationLimit.Max > 0 && value.Count > configuration.ValidationLimit.Max)
            {
                validationResults.Add(new ValidationResult(
                    _localizedTextService.Localize(
                        "validation",
                        "entriesExceed",
                        [configuration.ValidationLimit.Max.ToString(), (value.Count - configuration.ValidationLimit.Max).ToString()
                        ]),
                    ["value"]));
            }

            return validationResults;
        }
    }

    /// <summary>
    /// Validator to ensure that all selected elements are of an allowed content type, if any are configured.
    /// </summary>
    internal sealed class AllowedTypeValidator : ITypedJsonValidator<List<string>, ElementPickerConfiguration>
    {
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IElementService _elementService;
        private readonly ICoreScopeProvider _coreScopeProvider;

        public AllowedTypeValidator(
            ILocalizedTextService localizedTextService,
            IElementService elementService,
            ICoreScopeProvider coreScopeProvider)
        {
            _localizedTextService = localizedTextService;
            _elementService = elementService;
            _coreScopeProvider = coreScopeProvider;
        }

        public IEnumerable<ValidationResult> Validate(
            List<string>? value,
            ElementPickerConfiguration? configuration,
            string? valueType,
            PropertyValidationContext validationContext)
        {
            if (value is null || value.Count == 0 || configuration is null)
            {
                return [];
            }

            HashSet<Guid> allowedContentTypeKeys = ParseAllowedContentTypeKeys(configuration.AllowedContentTypeIds);

            // No filter configured — all element types are allowed.
            if (allowedContentTypeKeys.Count == 0)
            {
                return [];
            }

            Guid[] elementIds = value
                .Where(v => Guid.TryParse(v, out _))
                .Select(Guid.Parse)
                .ToArray();

            using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
            IElement[] elements = _elementService.GetByIds(elementIds).ToArray();
            scope.Complete();

            if (elements.Length != value.Count)
            {
                return [
                    new ValidationResult(
                        _localizedTextService.Localize("validation", "missingContent"),
                        ["value"])
                ];
            }

            foreach (IElement element in elements)
            {
                if (allowedContentTypeKeys.Contains(element.ContentType.Key) is false)
                {
                    return
                    [
                        new ValidationResult(
                            _localizedTextService.Localize("validation", "invalidObjectType"),
                            ["value"])
                    ];
                }
            }

            return [];
        }

        private static HashSet<Guid> ParseAllowedContentTypeKeys(string? configValue)
        {
            if (configValue.IsNullOrWhiteSpace())
            {
                return [];
            }

            var result = new HashSet<Guid>();
            foreach (var entry in configValue.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Guid.TryParse(entry, out Guid guid))
                {
                    result.Add(guid);
                }
            }

            return result;
        }
    }
}
