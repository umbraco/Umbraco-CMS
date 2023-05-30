using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a media picker property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MediaPicker3,
    EditorType.PropertyValue,
    "Media Picker",
    "mediapicker3",
    ValueType = ValueTypes.Json,
    Group = Constants.PropertyEditors.Groups.Media,
    Icon = Constants.Icons.MediaImage,
    ValueEditorIsReusable = true)]
public class MediaPicker3PropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public MediaPicker3PropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        EditorType type = EditorType.PropertyValue)
        : this(dataValueEditorFactory, ioHelper,
            StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>(), type)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPicker3PropertyEditor" /> class.
    /// </summary>
    public MediaPicker3PropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser,
        EditorType type = EditorType.PropertyValue)
        : base(dataValueEditorFactory, type)
    {
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory { get; } = new NoopPropertyIndexValueFactory();

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MediaPicker3ConfigurationEditor(_ioHelper, _editorConfigurationParser);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MediaPicker3PropertyValueEditor>(Attribute!);



    internal class MediaPicker3PropertyValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IMediaImportService _mediaImportService;
        private readonly IMediaService _mediaService;
        private readonly ITemporaryFileService _temporaryFileService;
        private readonly IScopeProvider _scopeProvider;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public MediaPicker3PropertyValueEditor(
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            IDataTypeService dataTypeService,
            IMediaImportService mediaImportService,
            IMediaService mediaService,
            ITemporaryFileService temporaryFileService,
            IScopeProvider scopeProvider,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
            _dataTypeService = dataTypeService;
            _mediaImportService = mediaImportService;
            _mediaService = mediaService;
            _temporaryFileService = temporaryFileService;
            _scopeProvider = scopeProvider;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        }

        /// <remarks>
        ///     Note: no FromEditor() and ToEditor() methods
        ///     We do not want to transform the way the data is stored in the DB and would like to keep a raw JSON string
        /// </remarks>
        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            foreach (MediaWithCropsDto dto in Deserialize(_jsonSerializer, value))
            {
                yield return new UmbracoEntityReference(Udi.Create(Constants.UdiEntityType.Media, dto.MediaKey));
            }
        }

        public override object ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);

            var dtos = Deserialize(_jsonSerializer, value).ToList();
            dtos = UpdateMediaTypeAliases(dtos);

            IDataType? dataType = _dataTypeService.GetDataType(property.PropertyType.DataTypeId);
            if (dataType?.ConfigurationObject != null)
            {
                MediaPicker3Configuration? configuration = dataType.ConfigurationAs<MediaPicker3Configuration>();

                foreach (MediaWithCropsDto dto in dtos)
                {
                    dto.ApplyConfiguration(configuration);
                }
            }

            return dtos;
        }

        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            // FIXME: consider creating an object deserialization method on IJsonSerializer instead of relying on deserializing serialized JSON here (and likely other places as well)
            if (editorValue.Value is not JsonArray jsonArray)
            {
                return base.FromEditor(editorValue, currentValue);
            }

            List<MediaWithCropsDto>? mediaWithCropsDtos = _jsonSerializer.Deserialize<List<MediaWithCropsDto>>(jsonArray.ToJsonString());
            if (mediaWithCropsDtos is null)
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
            Guid userKey = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Key
                         ?? throw new InvalidOperationException("Could not obtain the current backoffice user");

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

                GuidUdi? startNodeGuid = configuration.StartNodeId as GuidUdi ?? null;

                // make sure we'll clean up the temporary file if the scope completes
                using IScope scope = _scopeProvider.CreateScope();
                _temporaryFileService.EnlistDeleteIfScopeCompletes(temporaryFile.Key, _scopeProvider);

                // create a new media using the temporary file - the media type is passed from the client, in case
                // there are multiple allowed media types matching the file extension
                using Stream fileStream = temporaryFile.OpenReadStream();
                IMedia mediaFile = _mediaImportService
                    .ImportAsync(temporaryFile.FileName, fileStream, startNodeGuid?.Guid, mediaWithCropsDto.MediaTypeAlias, userKey)
                    .GetAwaiter()
                    .GetResult();

                mediaWithCropsDto.MediaKey = mediaFile.Key;
                scope.Complete();
            }

            return mediaWithCropsDtos.Except(invalidDtos).ToList();
        }

        /// <summary>
        ///     Model/DTO that represents the JSON that the MediaPicker3 stores.
        /// </summary>
        internal class MediaWithCropsDto
        {
            public Guid Key { get; set; }

            public Guid MediaKey { get; set; }

            public string MediaTypeAlias { get; set; } = string.Empty;

            public IEnumerable<ImageCropperValue.ImageCropperCrop>? Crops { get; set; }

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
    }
}
