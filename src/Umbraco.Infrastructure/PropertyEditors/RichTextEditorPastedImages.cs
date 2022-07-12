// Copyright (c) Umbraco.
// See LICENSE for more details.

using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
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
    }

    /// <summary>
    ///     Used by the RTE (and grid RTE) for drag/drop/persisting images
    /// </summary>
    public string FindAndPersistPastedTempImages(string html, Guid mediaParentFolder, int userId, IImageUrlGenerator imageUrlGenerator)
    {
        // Find all img's that has data-tmpimg attribute
        // Use HTML Agility Pack - https://html-agility-pack.net
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        HtmlNodeCollection? tmpImages = htmlDoc.DocumentNode.SelectNodes($"//img[@{TemporaryImageDataAttribute}]");
        if (tmpImages == null || tmpImages.Count == 0)
        {
            return html;
        }

        // An array to contain a list of URLs that
        // we have already processed to avoid dupes
        var uploadedImages = new Dictionary<string, GuidUdi>();

        foreach (HtmlNode? img in tmpImages)
        {
            // The data attribute contains the path to the tmp img to persist as a media item
            var tmpImgPath = img.GetAttributeValue(TemporaryImageDataAttribute, string.Empty);

            if (string.IsNullOrEmpty(tmpImgPath))
            {
                continue;
            }

            var absoluteTempImagePath = _hostingEnvironment.MapPathContentRoot(tmpImgPath);
            var fileName = Path.GetFileName(absoluteTempImagePath);
            var safeFileName = fileName.ToSafeFileName(_shortStringHelper);

            var mediaItemName = safeFileName.ToFriendlyName();
            IMedia mediaFile;
            GuidUdi udi;

            if (uploadedImages.ContainsKey(tmpImgPath) == false)
            {
                if (mediaParentFolder == Guid.Empty)
                {
                    mediaFile = _mediaService.CreateMedia(mediaItemName, Constants.System.Root, Constants.Conventions.MediaTypes.Image, userId);
                }
                else
                {
                    mediaFile = _mediaService.CreateMedia(mediaItemName, mediaParentFolder, Constants.Conventions.MediaTypes.Image, userId);
                }

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

                _mediaService.Save(mediaFile, userId);

                udi = mediaFile.GetUdi();
            }
            else
            {
                // Already been uploaded & we have it's UDI
                udi = uploadedImages[tmpImgPath];
            }

            // Add the UDI to the img element as new data attribute
            img.SetAttributeValue("data-udi", udi.ToString());

            // Get the new persisted image URL
            _umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext);
            IPublishedContent? mediaTyped = umbracoContext?.Media?.GetById(udi.Guid);
            if (mediaTyped == null)
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
                location = imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(location)
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
            if (uploadedImages.ContainsKey(tmpImgPath) == false)
            {
                uploadedImages.Add(tmpImgPath, udi);

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

        return htmlDoc.DocumentNode.OuterHtml;
    }
}
