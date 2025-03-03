// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a multi-node tree picker property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
    ValueType = ValueTypes.Text,
    ValueEditorIsReusable = true)]
public class MultiNodeTreePickerPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiNodeTreePickerPropertyEditor"/> class.
    /// </summary>
    public MultiNodeTreePickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc/>
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MultiNodePickerConfigurationEditor(_ioHelper);

    /// <inheritdoc/>
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultiNodeTreePickerPropertyValueEditor>(Attribute!);

    /// <summary>
    /// Defines the value editor for the media picker property editor.
    /// </summary>
    /// <remarks>
    /// At first glance, the FromEditor and ToEditor methods might seem strange.
    /// This is because we wanted to stop the leaking of UDIs to the frontend while not having to do database migrations
    /// so we opted to, for now, translate the UDI string in the database into a structured format unique to the client.
    /// This way, for now, no migration is needed and no changes outside of the editor logic needs to be touched to stop the leaking.
    /// </remarks>
    public class MultiNodeTreePickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiNodeTreePickerPropertyValueEditor"/> class.
        /// </summary>
        public MultiNodeTreePickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService,
            IEntityService entityService,
            ICoreScopeProvider coreScopeProvider,
            IContentService contentService,
            IMediaService mediaService,
            IMemberService memberService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
            Validators.Add(new TypedJsonValidatorRunner<EditorEntityReference[], MultiNodePickerConfiguration>(
                jsonSerializer,
                new MinMaxValidator(localizedTextService),
                new ObjectTypeValidator(localizedTextService, coreScopeProvider, entityService),
                new ContentTypeValidator(localizedTextService, coreScopeProvider, contentService, mediaService, memberService)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiNodeTreePickerPropertyValueEditor"/> class.
        /// </summary>
        [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 17.")]
        public MultiNodeTreePickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : this(
                shortStringHelper,
                jsonSerializer,
                ioHelper,
                attribute,
                StaticServiceProvider.Instance.GetRequiredService<ILocalizedTextService>(),
                StaticServiceProvider.Instance.GetRequiredService<IEntityService>(),
                StaticServiceProvider.Instance.GetRequiredService<ICoreScopeProvider>(),
                StaticServiceProvider.Instance.GetRequiredService<IContentService>(),
                StaticServiceProvider.Instance.GetRequiredService<IMediaService>(),
                StaticServiceProvider.Instance.GetRequiredService<IMemberService>())
        {
        }

        /// <inheritdoc/>
        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

            var udiPaths = asString!.Split(',');
            foreach (var udiPath in udiPaths)
            {
                if (UdiParser.TryParse(udiPath, out Udi? udi))
                {
                    yield return new UmbracoEntityReference(udi);
                }
            }
        }

        /// <inheritdoc/>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => editorValue.Value is JsonArray jsonArray
                ? EntityReferencesToUdis(_jsonSerializer.Deserialize<IEnumerable<EditorEntityReference>>(jsonArray.ToJsonString()) ?? Enumerable.Empty<EditorEntityReference>())
                : null;

        /// <inheritdoc/>
        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);
            return value is string stringValue
            ? UdisToEntityReferences(stringValue.Split(Constants.CharArrays.Comma)).ToArray()
            : null;
        }

        private static IEnumerable<EditorEntityReference> UdisToEntityReferences(IEnumerable<string> stringUdis)
        {
            foreach (var stringUdi in stringUdis)
            {
                if (UdiParser.TryParse(stringUdi, out GuidUdi? guidUdi) is false)
                {
                    continue;
                }

                yield return new EditorEntityReference() { Type = guidUdi.EntityType, Unique = guidUdi.Guid };
            }
        }

        private static string EntityReferencesToUdis(IEnumerable<EditorEntityReference> nodeReferences)
            => string.Join(",", nodeReferences.Select(entityReference => Udi.Create(entityReference.Type, entityReference.Unique).ToString()));

        /// <summary>
        /// Describes and editor entity reference.
        /// </summary>
        public class EditorEntityReference
        {
            /// <summary>
            /// Gets or sets the entity object type.
            /// </summary>
            public required string Type { get; set; }

            /// <summary>
            /// Gets or sets the entity unique identifier.
            /// </summary>
            public required Guid Unique { get; set; }
        }

        /// <summary>
        /// Gets the name of the configured object type for documents.
        /// </summary>
        internal const string DocumentObjectType = "content";

        /// <summary>
        /// Gets the name of the configured object type for media.
        /// </summary>
        internal const string MediaObjectType = "media";

        /// <summary>
        /// Gets the name of the configured object type for members.
        /// </summary>
        internal const string MemberObjectType = "member";

        /// <inheritdoc/>
        /// <summary>
        /// Validates the min/max configuration for the multi-node tree picker property editor.
        /// </summary>
        internal class MinMaxValidator : ITypedJsonValidator<EditorEntityReference[], MultiNodePickerConfiguration>
        {
            private readonly ILocalizedTextService _localizedTextService;

            /// <summary>
            /// Initializes a new instance of the <see cref="MinMaxValidator"/> class.
            /// </summary>
            public MinMaxValidator(ILocalizedTextService localizedTextService) => _localizedTextService = localizedTextService;

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(
                EditorEntityReference[]? entityReferences,
                MultiNodePickerConfiguration? configuration,
                string? valueType,
                PropertyValidationContext validationContext)
            {
                var validationResults = new List<ValidationResult>();

                if (entityReferences is null || configuration is null)
                {
                    return validationResults;
                }

                if (configuration.MinNumber > 0 && entityReferences.Length < configuration.MinNumber)
                {
                    validationResults.Add(new ValidationResult(
                        _localizedTextService.Localize(
                            "validation",
                            "entriesShort",
                            [configuration.MinNumber.ToString(), (configuration.MinNumber - entityReferences.Length).ToString()
                            ]),
                        ["value"]));
                }

                if (configuration.MaxNumber > 0 && entityReferences.Length > configuration.MaxNumber)
                {
                    validationResults.Add(new ValidationResult(
                        _localizedTextService.Localize(
                            "validation",
                            "entriesExceed",
                            [configuration.MaxNumber.ToString(), (entityReferences.Length - configuration.MaxNumber).ToString()
                            ]),
                        ["value"]));
                }

                return validationResults;
            }
        }

        /// <inheritdoc/>
        /// <summary>
        /// Validates the selected object type for the multi-node tree picker property editor.
        /// </summary>
        internal class ObjectTypeValidator : ITypedJsonValidator<EditorEntityReference[], MultiNodePickerConfiguration>
        {
            private readonly ILocalizedTextService _localizedTextService;
            private readonly ICoreScopeProvider _coreScopeProvider;
            private readonly IEntityService _entityService;

            /// <summary>
            /// Initializes a new instance of the <see cref="ObjectTypeValidator"/> class.
            /// </summary>
            public ObjectTypeValidator(
                ILocalizedTextService localizedTextService,
                ICoreScopeProvider coreScopeProvider,
                IEntityService entityService)
            {
                _localizedTextService = localizedTextService;
                _coreScopeProvider = coreScopeProvider;
                _entityService = entityService;
            }

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(
                EditorEntityReference[]? entityReferences,
                MultiNodePickerConfiguration? configuration,
                string? valueType,
                PropertyValidationContext validationContext)
            {
                var validationResults = new List<ValidationResult>();

                if (entityReferences is null || configuration?.TreeSource?.ObjectType is null)
                {
                    return validationResults;
                }

                Guid[] uniqueKeys = entityReferences.DistinctBy(x => x.Unique).Select(x => x.Unique).ToArray();

                if (uniqueKeys.Length == 0)
                {
                    return validationResults;
                }

                Guid? allowedObjectType = GetObjectType(configuration.TreeSource.ObjectType);
                if (allowedObjectType is null)
                {
                    return
                    [

                        // Some invalid object type was sent.
                        new ValidationResult(
                            _localizedTextService.Localize(
                                "validation",
                                "invalidObjectType"),
                            ["value"])
                    ];
                }

                using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
                foreach (Guid key in uniqueKeys)
                {
                    IEntitySlim? entity = _entityService.Get(key);
                    if (entity is not null && entity.NodeObjectType != allowedObjectType)
                    {
                        validationResults.Add(new ValidationResult(
                            _localizedTextService.Localize(
                                "validation",
                                "invalidObjectType"),
                            ["value"]));
                    }
                }

                scope.Complete();

                return validationResults;
            }

            private static Guid? GetObjectType(string objectType) =>
                objectType switch
                {
                    DocumentObjectType => Constants.ObjectTypes.Document,
                    MediaObjectType => Constants.ObjectTypes.Media,
                    MemberObjectType => Constants.ObjectTypes.Member,
                    _ => null,
                };
        }

        /// <inheritdoc/>
        /// <summary>
        /// Validates the selected content type for the multi-node tree picker property editor.
        /// </summary>
        internal class ContentTypeValidator : ITypedJsonValidator<EditorEntityReference[], MultiNodePickerConfiguration>
        {
            private readonly ILocalizedTextService _localizedTextService;
            private readonly ICoreScopeProvider _coreScopeProvider;
            private readonly IContentService _contentService;
            private readonly IMediaService _mediaService;
            private readonly IMemberService _memberService;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContentTypeValidator"/> class.
            /// </summary>
            public ContentTypeValidator(
                ILocalizedTextService localizedTextService,
                ICoreScopeProvider coreScopeProvider,
                IContentService contentService,
                IMediaService mediaService,
                IMemberService memberService)
            {
                _localizedTextService = localizedTextService;
                _coreScopeProvider = coreScopeProvider;
                _contentService = contentService;
                _mediaService = mediaService;
                _memberService = memberService;
            }

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(
                EditorEntityReference[]? entityReferences,
                MultiNodePickerConfiguration? configuration,
                string? valueType,
                PropertyValidationContext validationContext)
            {
                var validationResults = new List<ValidationResult>();

                Guid[] allowedTypes = configuration?.Filter?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToArray() ?? [];

                // We can't validate if there is no object type, and we don't need to if there's no filter.
                if (entityReferences is null || allowedTypes.Length == 0 || configuration?.TreeSource?.ObjectType is null)
                {
                    return validationResults;
                }

                using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

                Guid?[] uniqueContentTypeKeys = entityReferences
                    .Select(x => x.Unique)
                    .Distinct()
                    .Select(x => GetContent(configuration.TreeSource.ObjectType, x))
                    .Select(x => x?.ContentType.Key)
                    .Distinct()
                    .ToArray();

                scope.Complete();

                foreach (Guid? key in uniqueContentTypeKeys)
                {
                    if (key is null)
                    {
                        validationResults.Add(new ValidationResult(
                            _localizedTextService.Localize(
                                "validation",
                                "missingContent"),
                            ["value"]));
                        continue;
                    }

                    if (allowedTypes.Contains(key.Value) is false)
                    {
                        validationResults.Add(new ValidationResult(
                            _localizedTextService.Localize(
                                "validation",
                                "invalidObjectType"),
                            ["value"]));
                    }
                }

                return validationResults;
            }

            private IContentBase? GetContent(string objectType, Guid key) =>
                objectType switch
                {
                    DocumentObjectType => _contentService.GetById(key),
                    MediaObjectType => _mediaService.GetById(key),
                    MemberObjectType => _memberService.GetById(key),
                    _ => null,
                };
        }
    }
}
