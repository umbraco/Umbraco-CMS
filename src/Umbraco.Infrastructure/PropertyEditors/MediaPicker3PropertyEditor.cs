using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;
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

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MediaPicker3ConfigurationEditor(_ioHelper, _editorConfigurationParser);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MediaPicker3PropertyValueEditor>(Attribute!);

    internal class MediaPicker3PropertyValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly MediaFileManager _mediaFileManager;
        private readonly IMediaService _mediaService;
        private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly ILogger<MediaPicker3PropertyEditor> _logger;


        public MediaPicker3PropertyValueEditor(
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            IDataTypeService dataTypeService,
            IHostingEnvironment hostingEnvironment,
            MediaFileManager mediaFileManager,
            IMediaService mediaService,
            MediaUrlGeneratorCollection mediaUrlGenerators,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            ILogger<MediaPicker3PropertyEditor> logger)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
            _dataTypeService = dataTypeService;
            _hostingEnvironment = hostingEnvironment;
            _mediaFileManager = mediaFileManager;
            _mediaService = mediaService;
            _mediaUrlGenerators = mediaUrlGenerators;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _logger = logger;
            _shortStringHelper = shortStringHelper;
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

            IDataType? dataType = _dataTypeService.GetDataType(property.PropertyType.DataTypeId);
            if (dataType?.Configuration != null)
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
            if (editorValue.Value is JArray dtos)
            {
                dtos = PersistTempImages(dtos);
                // Clean up redundant/default data
                foreach (JObject? dto in dtos.Values<JObject>())
                {
                    MediaWithCropsDto.Prune(dto);
                }

                return dtos.ToString(Formatting.None);
            }

            return base.FromEditor(editorValue, currentValue);
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

        private JArray PersistTempImages(JArray jArray)
        {
            var result = new JArray();
            foreach (var dto in jArray.Values<JObject>())
            {
                TempImageDto? tempImageDto = _jsonSerializer.Deserialize<TempImageDto>(dto!.ToString());

                // If it has a temporary location, we have to persist it
                if (string.IsNullOrWhiteSpace(tempImageDto?.TmpLocation))
                {
                    result.Add(dto);
                    continue;
                }

                var tmpImgPath = tempImageDto!.TmpLocation;
                var absoluteTempImagePath = _hostingEnvironment.MapPathContentRoot(tmpImgPath);
                var fileName = Path.GetFileName(absoluteTempImagePath);
                var safeFileName = fileName.ToSafeFileName(_shortStringHelper);

                var mediaItemName = safeFileName.ToFriendlyName();
                GuidUdi udi;

                IMedia mediaFile = _mediaService.CreateMedia(mediaItemName, Constants.System.Root, Constants.Conventions.MediaTypes.Image, Constants.Security.SuperUserId);

                var fileInfo = new FileInfo(absoluteTempImagePath);

                FileStream? fileStream = fileInfo.OpenReadWithRetry();
                if (fileStream == null)
                {
                    throw new InvalidOperationException("Could not acquire file stream");
                }

                using (fileStream)
                {
                    mediaFile.SetValue(_mediaFileManager, _mediaUrlGenerators, _shortStringHelper, _contentTypeBaseServiceProvider, Constants.Conventions.Media.File, safeFileName, fileStream);
                }

                _mediaService.Save(mediaFile, Constants.Security.SuperUserId);

                udi = mediaFile.GetUdi();
                var mediaDto = new MediaWithCropsDto()
                {
                    Key = tempImageDto.Key,
                    MediaKey = mediaFile.GetUdi().Guid,
                    Crops = tempImageDto.Crops,
                    FocalPoint = tempImageDto.FocalPoint
                };

                result.Add(JObject.Parse(_jsonSerializer.Serialize(mediaDto)));

                // // // Add the UDI to the img element as new data attribute
                // // img.SetAttributeValue("data-udi", udi.ToString());
                //
                // // Get the new persisted image URL
                // _umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext);
                // IPublishedContent? mediaTyped = umbracoContext?.Media?.GetById(udi.Guid);
                // if (mediaTyped == null)
                // {
                //     throw new PanicException(
                //         $"Could not find media by id {udi.Guid} or there was no UmbracoContext available.");
                // }
                //
                // var location = mediaTyped.Url(_publishedUrlProvider);
                //
                // // Find the width & height attributes as we need to set the imageprocessor QueryString
                // var width = img.GetAttributeValue("width", int.MinValue);
                // var height = img.GetAttributeValue("height", int.MinValue);
                //
                // if (width != int.MinValue && height != int.MinValue)
                // {
                //     location = imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(location)
                //     {
                //         ImageCropMode = ImageCropMode.Max, Width = width, Height = height,
                //     });
                // }
                //
                // img.SetAttributeValue("src", location);
                //
                // // Remove the data attribute (so we do not re-process this)
                // img.Attributes.Remove(TemporaryImageDataAttribute);

                // Delete folder & image now its saved in media
                // The folder should contain one image - as a unique guid folder created
                // for each image uploaded from TinyMceController
                var folderName = Path.GetDirectoryName(absoluteTempImagePath);
                try
                {
                    if (folderName is not null)
                    {
                        Directory.Delete(folderName, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not delete temp file or folder {FileName}", absoluteTempImagePath);
                }
            }

            return result!;
        }

        [DataContract]
        internal class TempImageDto
        {
            [DataMember(Name = "key")] public Guid Key { get; set; }

            [DataMember(Name = "name")] public string Name { get; set; } = null!;

            [DataMember(Name = "tmpLocation")] public string TmpLocation { get; set; } = null!;

            [DataMember(Name = "crops")] public IEnumerable<ImageCropperValue.ImageCropperCrop>? Crops { get; set; }

            [DataMember(Name = "focalPoint")] public ImageCropperValue.ImageCropperFocalPoint? FocalPoint { get; set; }
        }

        /// <summary>
        ///     Model/DTO that represents the JSON that the MediaPicker3 stores.
        /// </summary>
        [DataContract]
        internal class MediaWithCropsDto
        {
            [DataMember(Name = "key")]
            public Guid Key { get; set; }

            [DataMember(Name = "mediaKey")]
            public Guid MediaKey { get; set; }

            [DataMember(Name = "crops")] public IEnumerable<ImageCropperValue.ImageCropperCrop>? Crops { get; set; }

            [DataMember(Name = "focalPoint")] public ImageCropperValue.ImageCropperFocalPoint? FocalPoint { get; set; }

            /// <summary>
            ///     Removes redundant crop data/default focal point.
            /// </summary>
            /// <param name="value">The media with crops DTO.</param>
            /// <remarks>
            ///     Because the DTO uses the same JSON keys as the image cropper value for crops and focal point, we can re-use the
            ///     prune method.
            /// </remarks>
            public static void Prune(JObject? value) => ImageCropperValue.Prune(value);

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
