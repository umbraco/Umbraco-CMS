using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a media picker property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MediaPicker3,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class MediaPicker3PropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPicker3PropertyEditor" /> class.
    /// </summary>
    public MediaPicker3PropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    public override IPropertyIndexValueFactory PropertyIndexValueFactory { get; } = new NoopPropertyIndexValueFactory();

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MediaPicker3ConfigurationEditor(_ioHelper);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MediaPicker3PropertyValueEditor>(Attribute!);

    /// <summary>
    /// Defines the value editor for the media picker property editor.
    /// </summary>
    internal class MediaPicker3PropertyValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IDataTypeConfigurationCache _dataTypeReadCache;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IMediaImportService _mediaImportService;
        private readonly IMediaService _mediaService;
        private readonly ITemporaryFileService _temporaryFileService;
        private readonly IScopeProvider _scopeProvider;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPicker3PropertyValueEditor"/> class.
        /// </summary>
        /// <remarks>
        ///     Note on FromEditor() and ToEditor() methods.
        ///     We do not want to transform the way the data is stored in the DB and would like to keep a raw JSON string.
        /// </remarks>
        public MediaPicker3PropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            IMediaImportService mediaImportService,
            IMediaService mediaService,
            ITemporaryFileService temporaryFileService,
            IScopeProvider scopeProvider,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IDataTypeConfigurationCache dataTypeReadCache,
            ILocalizedTextService localizedTextService,
            IMediaTypeService mediaTypeService,
            IMediaNavigationQueryService mediaNavigationQueryService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
            _mediaImportService = mediaImportService;
            _mediaService = mediaService;
            _temporaryFileService = temporaryFileService;
            _scopeProvider = scopeProvider;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _dataTypeReadCache = dataTypeReadCache;
            var validators = new TypedJsonValidatorRunner<List<MediaWithCropsDto>, MediaPicker3Configuration>(
                jsonSerializer,
                new MinMaxValidator(localizedTextService),
                new AllowedTypeValidator(localizedTextService, mediaTypeService, _mediaService),
                new StartNodeValidator(localizedTextService, mediaNavigationQueryService));

            Validators.Add(validators);
        }

        /// <inheritdoc/>
        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            foreach (MediaWithCropsDto dto in Deserialize(_jsonSerializer, value))
            {
                yield return new UmbracoEntityReference(Udi.Create(Constants.UdiEntityType.Media, dto.MediaKey));
            }
        }

        /// <inheritdoc/>
        public override object ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);

            var dtos = Deserialize(_jsonSerializer, value).ToList();
            dtos = UpdateMediaTypeAliases(dtos);

            MediaPicker3Configuration? configuration = _dataTypeReadCache.GetConfigurationAs<MediaPicker3Configuration>(property.PropertyType.DataTypeKey);
            if (configuration is not null)
            {
                foreach (MediaWithCropsDto dto in dtos)
                {
                    dto.ApplyConfiguration(configuration);
                }
            }

            return dtos;
        }

        /// <inheritdoc/>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            if (editorValue.Value is null ||
                _jsonSerializer.TryDeserialize(editorValue.Value, out List<MediaWithCropsDto>? mediaWithCropsDtos) is false)
            {
                return base.FromEditor(editorValue, currentValue);
            }

            if (editorValue.DataTypeConfiguration is MediaPicker3Configuration configuration)
            {
                // handle temporary media uploads
                mediaWithCropsDtos = HandleTemporaryMediaUploads(mediaWithCropsDtos, configuration);
            }

            foreach (MediaWithCropsDto mediaWithCropsDto in mediaWithCropsDtos)
            {
                mediaWithCropsDto.Prune();
            }

            return _jsonSerializer.Serialize(mediaWithCropsDtos);
        }

        /// <summary>
        /// Deserializes the provided JSON value into a list of <see cref="MediaWithCropsDto"/>.
        /// </summary>
        internal static IEnumerable<MediaWithCropsDto> Deserialize(IJsonSerializer jsonSerializer, object? value)
        {
            var rawJson = value is string str ? str : value?.ToString();
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                yield break;
            }

            if (!rawJson.DetectIsJson())
            {
                // Old comma seperated UDI format
                foreach (var udiStr in rawJson.Split(Constants.CharArrays.Comma))
                {
                    if (UdiParser.TryParse(udiStr, out Udi? udi) && udi is GuidUdi guidUdi)
                    {
                        yield return new MediaWithCropsDto
                        {
                            Key = Guid.NewGuid(),
                            MediaKey = guidUdi.Guid,
                            Crops = Enumerable.Empty<ImageCropperValue.ImageCropperCrop>(),
                            FocalPoint = new ImageCropperValue.ImageCropperFocalPoint {Left = 0.5m, Top = 0.5m},
                        };
                    }
                }
            }
            else
            {
                IEnumerable<MediaWithCropsDto>? dtos =
                    jsonSerializer.Deserialize<IEnumerable<MediaWithCropsDto>>(rawJson);
                if (dtos is not null)
                {
                    // New JSON format
                    foreach (MediaWithCropsDto dto in dtos)
                    {
                        yield return dto;
                    }
                }
            }
        }

        private List<MediaWithCropsDto> UpdateMediaTypeAliases(List<MediaWithCropsDto> mediaWithCropsDtos)
        {
            const string unknownMediaType = "UNKNOWN";

            foreach (MediaWithCropsDto mediaWithCropsDto in mediaWithCropsDtos)
            {
                IMedia? media = _mediaService.GetById(mediaWithCropsDto.MediaKey);
                mediaWithCropsDto.MediaTypeAlias = media?.ContentType.Alias ?? unknownMediaType;
            }

            return mediaWithCropsDtos.Where(m => m.MediaTypeAlias != unknownMediaType).ToList();
        }

        private List<MediaWithCropsDto> HandleTemporaryMediaUploads(List<MediaWithCropsDto> mediaWithCropsDtos, MediaPicker3Configuration configuration)
        {
            var invalidDtos = new List<MediaWithCropsDto>();

            foreach (MediaWithCropsDto mediaWithCropsDto in mediaWithCropsDtos)
            {
                // if the media already exist, don't bother with it
                if (_mediaService.GetById(mediaWithCropsDto.MediaKey) != null)
                {
                    continue;
                }

                // we'll assume that the media key is the key of a temporary file
                TemporaryFileModel? temporaryFile = _temporaryFileService.GetAsync(mediaWithCropsDto.MediaKey).GetAwaiter().GetResult();
                if (temporaryFile == null)
                {
                    // the temporary file is missing, don't process this item any further
                    invalidDtos.Add(mediaWithCropsDto);
                    continue;
                }

                Guid? startNodeGuid = configuration.StartNodeId;

                // make sure we'll clean up the temporary file if the scope completes
                using IScope scope = _scopeProvider.CreateScope();
                _temporaryFileService.EnlistDeleteIfScopeCompletes(temporaryFile.Key, _scopeProvider);

                // create a new media using the temporary file - the media type is passed from the client, in case
                // there are multiple allowed media types matching the file extension
                using Stream fileStream = temporaryFile.OpenReadStream();
                IMedia mediaFile = _mediaImportService
                    .ImportAsync(temporaryFile.FileName, fileStream, startNodeGuid, mediaWithCropsDto.MediaTypeAlias, CurrentUserKey())
                    .GetAwaiter()
                    .GetResult();

                mediaWithCropsDto.MediaKey = mediaFile.Key;
                scope.Complete();
            }

            return mediaWithCropsDtos.Except(invalidDtos).ToList();
        }

        private Guid CurrentUserKey() => _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Key
                                         ?? throw new InvalidOperationException("Could not obtain the current backoffice user");

        /// <summary>
        ///     Model/DTO that represents the JSON that the MediaPicker3 stores.
        /// </summary>
        internal class MediaWithCropsDto
        {
            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            public Guid Key { get; set; }

            /// <summary>
            /// Gets or sets the media key.
            /// </summary>
            public Guid MediaKey { get; set; }

            /// <summary>
            /// Gets or sets the media type alias.
            /// </summary>
            public string MediaTypeAlias { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the crops.
            /// </summary>
            public IEnumerable<ImageCropperValue.ImageCropperCrop>? Crops { get; set; }

            /// <summary>
            /// Gets or sets the focal point.
            /// </summary>
            public ImageCropperValue.ImageCropperFocalPoint? FocalPoint { get; set; }

            /// <summary>
            ///     Removes redundant crop data/default focal point.
            /// </summary>
            /// <remarks>
            ///     Because the DTO uses the same JSON keys as the image cropper value for crops and focal point, we can re-use the
            ///     prune method.
            /// </remarks>
            internal void Prune()
            {
                Crops = Crops?.Where(crop => crop.Coordinates != null).ToArray();
                if (FocalPoint is { Top: 0.5m, Left: 0.5m })
                {
                    FocalPoint = null;
                }
            }

            /// <summary>
            ///     Applies the configuration to ensure only valid crops are kept and have the correct width/height.
            /// </summary>
            /// <param name="configuration">The configuration.</param>
            public void ApplyConfiguration(MediaPicker3Configuration? configuration)
            {
                var crops = new List<ImageCropperValue.ImageCropperCrop>();

                MediaPicker3Configuration.CropConfiguration[]? configuredCrops = configuration?.Crops;
                if (configuredCrops != null)
                {
                    foreach (MediaPicker3Configuration.CropConfiguration configuredCrop in configuredCrops)
                    {
                        ImageCropperValue.ImageCropperCrop? crop =
                            Crops?.FirstOrDefault(x => x.Alias == configuredCrop.Alias);

                        crops.Add(new ImageCropperValue.ImageCropperCrop
                        {
                            Alias = configuredCrop.Alias,
                            Width = configuredCrop.Width,
                            Height = configuredCrop.Height,
                            Coordinates = crop?.Coordinates,
                        });
                    }
                }

                Crops = crops;

                if (configuration?.EnableLocalFocalPoint == false)
                {
                    FocalPoint = null;
                }
            }
        }

        /// <summary>
        /// Validates the min/max configuration for the media picker property editor.
        /// </summary>
        internal class MinMaxValidator : ITypedJsonValidator<List<MediaWithCropsDto>, MediaPicker3Configuration>
        {
            private readonly ILocalizedTextService _localizedTextService;

            /// <summary>
            /// Initializes a new instance of the <see cref="MinMaxValidator"/> class.
            /// </summary>
            public MinMaxValidator(ILocalizedTextService localizedTextService) => _localizedTextService = localizedTextService;

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(
                List<MediaWithCropsDto>? mediaWithCropsDtos,
                MediaPicker3Configuration? mediaPickerConfiguration,
                string? valueType,
                PropertyValidationContext validationContext)
            {
                var validationResults = new List<ValidationResult>();

                if (mediaWithCropsDtos is null || mediaPickerConfiguration is null)
                {
                    return validationResults;
                }

                if (mediaPickerConfiguration.Multiple is false && mediaWithCropsDtos.Count > 1)
                {
                    validationResults.Add(new ValidationResult(
                        _localizedTextService.Localize("validation", "multipleMediaNotAllowed"),
                        ["value"]));
                }

                if (mediaPickerConfiguration.ValidationLimit.Min is not null
                    && mediaWithCropsDtos.Count < mediaPickerConfiguration.ValidationLimit.Min)
                {
                    validationResults.Add(new ValidationResult(
                        _localizedTextService.Localize(
                            "validation",
                            "entriesShort",
                            [mediaPickerConfiguration.ValidationLimit.Min.ToString(), (mediaPickerConfiguration.ValidationLimit.Min - mediaWithCropsDtos.Count).ToString()
                            ]),
                        ["value"]));
                }

                if (mediaPickerConfiguration.ValidationLimit.Max is not null
                    && mediaWithCropsDtos.Count > mediaPickerConfiguration.ValidationLimit.Max)
                {
                    validationResults.Add(new ValidationResult(
                        _localizedTextService.Localize(
                            "validation",
                            "entriesExceed",
                            [mediaPickerConfiguration.ValidationLimit.Max.ToString(), (mediaWithCropsDtos.Count - mediaPickerConfiguration.ValidationLimit.Max).ToString()
                            ]),
                        ["value"]));
                }

                return validationResults;
            }
        }

        /// <summary>
        /// Validates the allowed type configuration for the media picker property editor.
        /// </summary>
        internal class AllowedTypeValidator : ITypedJsonValidator<List<MediaWithCropsDto>, MediaPicker3Configuration>
        {
            private readonly ILocalizedTextService _localizedTextService;
            private readonly IMediaTypeService _mediaTypeService;
            private readonly IMediaService _mediaService;

            /// <summary>
            /// Initializes a new instance of the <see cref="AllowedTypeValidator"/> class.
            /// </summary>
            public AllowedTypeValidator(ILocalizedTextService localizedTextService, IMediaTypeService mediaTypeService, IMediaService mediaService)
            {
                _localizedTextService = localizedTextService;
                _mediaTypeService = mediaTypeService;
                _mediaService = mediaService;
            }

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(
                List<MediaWithCropsDto>? value,
                MediaPicker3Configuration? configuration,
                string? valueType,
                PropertyValidationContext validationContext)
            {
                if (value is null || configuration is null)
                {
                    return [];
                }

                var allowedTypes = configuration.Filter?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);

                // No allowed types = all types are allowed
                if (allowedTypes is null || allowedTypes.Length == 0)
                {
                    return [];
                }

                // We may or may not have explicit MediaTypeAlias values provided, depending on whether the operation is an update or a
                // create. So let's make sure we have them all.
                IEnumerable<string> providedTypeAliases = value
                    .Where(x => x.MediaTypeAlias.IsNullOrWhiteSpace() is false)
                    .Select(x => x.MediaTypeAlias);

                IEnumerable<Guid> retrievedMediaKeys = value
                    .Where(x => x.MediaTypeAlias.IsNullOrWhiteSpace())
                    .Select(x => x.MediaKey);
                IEnumerable<IMedia> retrievedMedia = _mediaService.GetByIds(retrievedMediaKeys);
                IEnumerable<string> retrievedTypeAliases = retrievedMedia
                    .Select(x => x.ContentType.Alias);

                IEnumerable<string> distinctTypeAliases = providedTypeAliases.Union(retrievedTypeAliases).Distinct();

                foreach (var typeAlias in distinctTypeAliases)
                {
                    IMediaType? type = _mediaTypeService.Get(typeAlias);

                    if (type is null || allowedTypes.Contains(type.Key.ToString()) is false)
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
        }

        /// <summary>
        /// Validates the start node configuration for the media picker property editor.
        /// </summary>
        internal class StartNodeValidator : ITypedJsonValidator<List<MediaWithCropsDto>, MediaPicker3Configuration>
        {
            private readonly ILocalizedTextService _localizedTextService;
            private readonly IMediaNavigationQueryService _mediaNavigationQueryService;

            /// <summary>
            /// Initializes a new instance of the <see cref="StartNodeValidator"/> class.
            /// </summary>
            public StartNodeValidator(
                ILocalizedTextService localizedTextService,
                IMediaNavigationQueryService mediaNavigationQueryService)
            {
                _localizedTextService = localizedTextService;
                _mediaNavigationQueryService = mediaNavigationQueryService;
            }

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(
                List<MediaWithCropsDto>? value,
                MediaPicker3Configuration? configuration,
                string? valueType,
                PropertyValidationContext validationContext)
            {
                if (value is null || configuration?.StartNodeId is null)
                {
                    return [];
                }

                if (ValidationHelper.HasValidStartNode(value.Select(x => x.MediaKey), configuration.StartNodeId.Value, _mediaNavigationQueryService) is false)
                {
                    return
                    [
                        new ValidationResult(
                            _localizedTextService.Localize("validation", "invalidStartNode"),
                            ["value"])
                    ];
                }

                return [];
            }
        }
    }
}
