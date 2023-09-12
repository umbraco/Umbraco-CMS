// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public sealed class RichTextEditorPastedImages
{
    private const string TemporaryImageDataAttribute = "data-tmpimg";
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<RichTextEditorPastedImages> _logger;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IMediaService _mediaService;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly string _tempFolderAbsolutePath;
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly ContentSettings _contentSettings;
    private readonly Dictionary<string, GuidUdi> _uploadedImages = new();

    [Obsolete("Use the ctor which takes an IImageUrlGenerator and IOptions<ContentSettings> instead, scheduled for removal in v14")]
    public RichTextEditorPastedImages(
        IUmbracoContextAccessor umbracoContextAccessor,
        ILogger<RichTextEditorPastedImages> logger,
        IHostingEnvironment hostingEnvironment,
        IMediaService mediaService,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IShortStringHelper shortStringHelper,
        IPublishedUrlProvider publishedUrlProvider)
        : this(
            umbracoContextAccessor,
            logger,
            hostingEnvironment,
            mediaService,
            contentTypeBaseServiceProvider,
            mediaFileManager,
            mediaUrlGenerators,
            shortStringHelper,
            publishedUrlProvider,
            StaticServiceProvider.Instance.GetRequiredService<IImageUrlGenerator>(),
            StaticServiceProvider.Instance.GetRequiredService<IOptions<ContentSettings>>())
    {
    }

    public RichTextEditorPastedImages(
        IUmbracoContextAccessor umbracoContextAccessor,
        ILogger<RichTextEditorPastedImages> logger,
        IHostingEnvironment hostingEnvironment,
        IMediaService mediaService,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IShortStringHelper shortStringHelper,
        IPublishedUrlProvider publishedUrlProvider,
        IImageUrlGenerator imageUrlGenerator,
        IOptions<ContentSettings> contentSettings)
    {
        _umbracoContextAccessor =
            umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hostingEnvironment = hostingEnvironment;
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider ??
                                          throw new ArgumentNullException(nameof(contentTypeBaseServiceProvider));
        _mediaFileManager = mediaFileManager;
        _mediaUrlGenerators = mediaUrlGenerators;
        _shortStringHelper = shortStringHelper;
        _publishedUrlProvider = publishedUrlProvider;
        _imageUrlGenerator = imageUrlGenerator;
        _contentSettings = contentSettings.Value;

        _tempFolderAbsolutePath = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempImageUploads);
    }

    /// <summary>
    ///     Used by the RTE (and grid RTE) for converting inline base64 images to Media items.
    /// </summary>
    /// <param name="html">HTML from the Rich Text Editor property editor.</param>
    /// <param name="mediaParentFolder"></param>
    /// <param name="userId"></param>
    /// <returns>Formatted HTML.</returns>
    /// <exception cref="NotSupportedException">Thrown if image extension is not allowed</exception>
    internal string FindAndPersistEmbeddedImages(string html, Guid mediaParentFolder, int userId)
    {
        // Find all img's that has data-tmpimg attribute
        // Use HTML Agility Pack - https://html-agility-pack.net
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        HtmlNodeCollection? imagesWithDataUris = htmlDoc.DocumentNode.SelectNodes("//img");
        if (imagesWithDataUris is null || imagesWithDataUris.Count is 0)
        {
            return html;
        }

        foreach (HtmlNode? img in imagesWithDataUris)
        {
            var srcValue = img.GetAttributeValue("src", string.Empty);

            // Ignore src-less images
            if (string.IsNullOrEmpty(srcValue))
            {
                continue;
            }

            // Take only images that have a "data:image" uri into consideration
            if (!srcValue.StartsWith("data:image"))
            {
                continue;
            }

            // Create tmp image by scanning the srcValue
            // the value will look like "data:image/jpg;base64,abc" where the first part
            // is the mimetype and the second (after the comma) is the image blob
            Match dataUriInfo = Regex.Match(srcValue, @"^data:\w+\/(?<ext>\w+)[\w\+]*?;(?<encoding>\w+),(?<data>.+)$");

            // If it turns up false, it was probably a false-positive and we can't do anything with it
            if (dataUriInfo.Success is false)
            {
                continue;
            }

            var ext = dataUriInfo.Groups["ext"].Value.ToLowerInvariant();
            var encoding = dataUriInfo.Groups["encoding"].Value.ToLowerInvariant();
            var imageData = dataUriInfo.Groups["data"].Value;

            if (_contentSettings.IsFileAllowedForUpload(ext) is false)
            {
                // If the image format is not supported we should probably leave it be
                // since the user decided to include it.
                // If we accepted it anyway, they could technically circumvent the allow list for file types,
                // but the user experience would not be very good if we simply failed to save the content.
                // Besides, there may be other types of data uri images technically supported by a browser that we cannot handle.
                _logger.LogWarning(
                    "Performance impact: Could not convert embedded image to a Media item because the file extension {Ext} was not allowed. HTML extract: {OuterHtml}",
                    ext,
                    img.OuterHtml.Length < 100 ? img.OuterHtml : img.OuterHtml[..100]); // only log the first 100 chars because base64 images can be very long
                continue;
            }

            // Create an unique folder path to help with concurrent users to avoid filename clash
            var imageTempPath =
                _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempImageUploads + Path.DirectorySeparatorChar + Guid.NewGuid());

            // Ensure image temp path exists
            if (Directory.Exists(imageTempPath) is false)
            {
                Directory.CreateDirectory(imageTempPath);
            }

            // To get the filename, we simply manipulate the mimetype into a filename
            var filePath = $"image.{ext}";
            var safeFileName = filePath.ToSafeFileName(_shortStringHelper);
            var tmpImgPath = imageTempPath + Path.DirectorySeparatorChar + safeFileName;
            var absoluteTempImagePath = Path.GetFullPath(tmpImgPath);

            // Convert the base64 content to a byte array and save the bytes directly to a file
            // this method should work for most use-cases
            if (encoding.Equals("base64"))
            {
                System.IO.File.WriteAllBytes(absoluteTempImagePath, Convert.FromBase64String(imageData));
            }
            else
            {
                System.IO.File.WriteAllText(absoluteTempImagePath, HttpUtility.HtmlDecode(imageData), Encoding.UTF8);
            }

            // When the temp file has been created, we can persist it
            PersistMediaItem(mediaParentFolder, userId, img, tmpImgPath);
        }

        return htmlDoc.DocumentNode.OuterHtml;
    }

    /// <summary>
    ///     Used by the RTE (and grid RTE) for drag/drop/persisting images.
    /// </summary>
    /// <param name="html">HTML from the Rich Text Editor property editor.</param>
    /// <param name="mediaParentFolder"></param>
    /// <param name="userId"></param>
    /// <param name="imageUrlGenerator"></param>
    /// <returns>Formatted HTML.</returns>
    [Obsolete("It is not needed to supply the imageUrlGenerator parameter")]
    public string FindAndPersistPastedTempImages(string html, Guid mediaParentFolder, int userId, IImageUrlGenerator imageUrlGenerator) =>
        FindAndPersistPastedTempImages(html, mediaParentFolder, userId);

    /// <summary>
    ///     Used by the RTE (and grid RTE) for drag/drop/persisting images.
    /// </summary>
    /// <param name="html">HTML from the Rich Text Editor property editor.</param>
    /// <param name="mediaParentFolder"></param>
    /// <param name="userId"></param>
    /// <returns>Formatted HTML.</returns>
    public string FindAndPersistPastedTempImages(string html, Guid mediaParentFolder, int userId)
    {
        // Find all img's that has data-tmpimg attribute
        // Use HTML Agility Pack - https://html-agility-pack.net
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        HtmlNodeCollection? tmpImages = htmlDoc.DocumentNode.SelectNodes($"//img[@{TemporaryImageDataAttribute}]");
        if (tmpImages is null || tmpImages.Count is 0)
        {
            return html;
        }

        foreach (HtmlNode? img in tmpImages)
        {
            // The data attribute contains the path to the tmp img to persist as a media item
            var tmpImgPath = img.GetAttributeValue(TemporaryImageDataAttribute, string.Empty);

            if (string.IsNullOrEmpty(tmpImgPath))
            {
                continue;
            }

            var qualifiedTmpImgPath = _hostingEnvironment.MapPathContentRoot(tmpImgPath);

            PersistMediaItem(mediaParentFolder, userId, img, qualifiedTmpImgPath);
        }

        return htmlDoc.DocumentNode.OuterHtml;
    }

    private void PersistMediaItem(Guid mediaParentFolder, int userId, HtmlNode img, string qualifiedTmpImgPath)
    {
        var absoluteTempImagePath = Path.GetFullPath(qualifiedTmpImgPath);

        if (IsValidPath(absoluteTempImagePath) is false)
        {
            return;
        }

        var fileName = Path.GetFileName(absoluteTempImagePath);
        var safeFileName = fileName.ToSafeFileName(_shortStringHelper);

        var mediaItemName = safeFileName.ToFriendlyName();
        GuidUdi udi;

        if (_uploadedImages.ContainsKey(qualifiedTmpImgPath) is false)
        {
            var isSvg = qualifiedTmpImgPath.EndsWith(".svg");
            var mediaType = isSvg
                ? Constants.Conventions.MediaTypes.VectorGraphicsAlias
                : Constants.Conventions.MediaTypes.Image;

            IMedia mediaFile = mediaParentFolder == Guid.Empty
                ? _mediaService.CreateMedia(mediaItemName, Constants.System.Root, mediaType, userId)
                : _mediaService.CreateMedia(mediaItemName, mediaParentFolder, mediaType, userId);

            var fileInfo = new FileInfo(absoluteTempImagePath);

            FileStream? fileStream = fileInfo.OpenReadWithRetry();
            if (fileStream is null)
            {
                throw new InvalidOperationException("Could not acquire file stream");
            }

            using (fileStream)
            {
                mediaFile.SetValue(_mediaFileManager, _mediaUrlGenerators, _shortStringHelper,
                    _contentTypeBaseServiceProvider, Constants.Conventions.Media.File, safeFileName, fileStream);
            }

            _mediaService.Save(mediaFile, userId);

            udi = mediaFile.GetUdi();
        }
        else
        {
            // Already been uploaded & we have it's UDI
            udi = _uploadedImages[qualifiedTmpImgPath];
        }

        // Add the UDI to the img element as new data attribute
        img.SetAttributeValue("data-udi", udi.ToString());

        // Get the new persisted image URL
        _umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext);
        IPublishedContent? mediaTyped = umbracoContext?.Media?.GetById(udi.Guid);
        if (mediaTyped is null)
        {
            throw new PanicException(
                $"Could not find media by id {udi.Guid} or there was no UmbracoContext available.");
        }

        var location = mediaTyped.Url(_publishedUrlProvider);

        // Find the width & height attributes as we need to set the imageprocessor QueryString
        var width = img.GetAttributeValue("width", int.MinValue);
        var height = img.GetAttributeValue("height", int.MinValue);

        if (width != int.MinValue && height != int.MinValue)
        {
            location = _imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(location)
            {
                ImageCropMode = ImageCropMode.Max,
                Width = width,
                Height = height,
            });
        }

        img.SetAttributeValue("src", location);

        // Remove the data attribute (so we do not re-process this)
        img.Attributes.Remove(TemporaryImageDataAttribute);

        // Add to the dictionary to avoid dupes
        if (_uploadedImages.ContainsKey(qualifiedTmpImgPath) is false)
        {
            _uploadedImages.Add(qualifiedTmpImgPath, udi);

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
    }

    private bool IsValidPath(string imagePath) => imagePath.StartsWith(_tempFolderAbsolutePath);
}
