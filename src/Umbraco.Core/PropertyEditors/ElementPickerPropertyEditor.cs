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
            Validators.Add(new ElementPickerValidatorRunner(
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

    internal sealed class ElementPickerValidatorRunner : IValueValidator
    {
        private readonly AllowedTypeValidator _validator;

        public ElementPickerValidatorRunner(AllowedTypeValidator validator)
            => _validator = validator;

        public IEnumerable<ValidationResult> Validate(
            object? value,
            string? valueType,
            object? dataTypeConfiguration,
            PropertyValidationContext validationContext)
        {
            if (dataTypeConfiguration is not ElementPickerConfiguration configuration)
            {
                return [];
            }

            Guid[]? guids = value is IEnumerable<string> strings
                ? strings.Select(s => Guid.TryParse(s, out Guid g) ? g : (Guid?)null).Where(g => g.HasValue).Select(g => g!.Value).ToArray()
                : null;

            return _validator.Validate(guids, configuration, valueType, validationContext);
        }
    }
    /// <summary>
    /// Validator for the allowed types of the element picker, validating that the selected elements are of an allowed content type.
    /// </summary>
    internal sealed class AllowedTypeValidator : ITypedJsonValidator<Guid[], ElementPickerConfiguration>
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
            Guid[]? value,
            ElementPickerConfiguration? configuration,
            string? valueType,
            PropertyValidationContext validationContext)
        {
            if (value is null || value.Length == 0 || configuration is null)
            {
                return [];
            }

            HashSet<Guid> allowedContentTypeKeys = ParseAllowedContentTypeKeys(configuration.AllowedContentTypeIds);

            // No filter configured — all element types are allowed.
            if (allowedContentTypeKeys.Count == 0)
            {
                return [];
            }

            using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
            IElement[] elements = _elementService.GetByIds(value).ToArray();
            scope.Complete();

            if (elements.Length != value.Length)
            {
                return [
                    new ValidationResult(
                        _localizedTextService.Localize("validation", "invalidObjectType"),
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
