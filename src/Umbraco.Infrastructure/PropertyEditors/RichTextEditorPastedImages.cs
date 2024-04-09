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
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public sealed class RichTextEditorPastedImages
{
    private const string TemporaryImageDataAttribute = "data-tmpimg";
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ILogger<RichTextEditorPastedImages> _logger;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IScopeProvider _scopeProvider;
    private readonly IMediaImportService _mediaImportService;
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly IUserService _userService;
    private readonly ContentSettings _contentSettings;

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V16.")]
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
            StaticServiceProvider.Instance.GetRequiredService<ITemporaryFileService>(),
            StaticServiceProvider.Instance.GetRequiredService<IScopeProvider>(),
            StaticServiceProvider.Instance.GetRequiredService<IMediaImportService>(),
            StaticServiceProvider.Instance.GetRequiredService<IImageUrlGenerator>(),
            StaticServiceProvider.Instance.GetRequiredService<IOptions<ContentSettings>>())
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V16.")]
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
        ITemporaryFileService temporaryFileService,
        IScopeProvider scopeProvider,
        IMediaImportService mediaImportService,
        IImageUrlGenerator imageUrlGenerator,
        IOptions<ContentSettings> contentSettings)
        : this(umbracoContextAccessor, logger, shortStringHelper, publishedUrlProvider, temporaryFileService, scopeProvider, mediaImportService, imageUrlGenerator, contentSettings)
    {
    }

    public RichTextEditorPastedImages(
        IUmbracoContextAccessor umbracoContextAccessor,
        ILogger<RichTextEditorPastedImages> logger,
        IShortStringHelper shortStringHelper,
        IPublishedUrlProvider publishedUrlProvider,
        ITemporaryFileService temporaryFileService,
        IScopeProvider scopeProvider,
        IMediaImportService mediaImportService,
        IImageUrlGenerator imageUrlGenerator,
        IOptions<ContentSettings> contentSettings)
    {
        _umbracoContextAccessor =
            umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
        _logger = logger;
        _shortStringHelper = shortStringHelper;
        _publishedUrlProvider = publishedUrlProvider;
        _temporaryFileService = temporaryFileService;
        _scopeProvider = scopeProvider;
        _mediaImportService = mediaImportService;
        _imageUrlGenerator = imageUrlGenerator;
        _contentSettings = contentSettings.Value;

        // this obviously is not correct. however, we only use IUserService in an obsolete method,
        // so this is better than having even more obsolete constructors for V16
        _userService = StaticServiceProvider.Instance.GetRequiredService<IUserService>();
    }

    /// <summary>
    ///     Used by the RTE (and grid RTE) for converting inline base64 images to Media items.
    /// </summary>
    /// <param name="html">HTML from the Rich Text Editor property editor.</param>
    /// <param name="mediaParentFolder"></param>
    /// <param name="userKey"></param>
    /// <returns>Formatted HTML.</returns>
    /// <exception cref="NotSupportedException">Thrown if image extension is not allowed</exception>
    internal async Task<string> FindAndPersistEmbeddedImagesAsync(string html, Guid mediaParentFolder, Guid userKey)
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

            // convert the encoded image data to bytes
            var bytes = encoding.Equals("base64")
                ? Convert.FromBase64String(imageData)
                : Encoding.UTF8.GetBytes(HttpUtility.HtmlDecode(imageData));
            GuidUdi udi;
            using (var stream = new MemoryStream(bytes))
            {
                var safeFileName = $"image.{ext}".ToSafeFileName(_shortStringHelper);
                var mediaTypeAlias = MediaTypeAlias(safeFileName);

                Guid? parentFolderKey = mediaParentFolder == Guid.Empty ? Constants.System.RootKey : mediaParentFolder;
                IMedia mediaFile = await _mediaImportService.ImportAsync(safeFileName, stream, parentFolderKey, mediaTypeAlias, userKey);
                udi = mediaFile.GetUdi();
            }

            UpdateImageNode(img, udi);
        }

        return htmlDoc.DocumentNode.OuterHtml;
    }

    [Obsolete($"Please use {nameof(FindAndPersistPastedTempImagesAsync)}. Will be removed in V16.")]
    public string FindAndPersistPastedTempImages(string html, Guid mediaParentFolder, int userId, IImageUrlGenerator imageUrlGenerator)
        => FindAndPersistPastedTempImages(html, mediaParentFolder, userId);

    [Obsolete($"Please use {nameof(FindAndPersistPastedTempImagesAsync)}. Will be removed in V16.")]
    public string FindAndPersistPastedTempImages(string html, Guid mediaParentFolder, int userId)
    {
        IUser user = _userService.GetUserById(userId)
                     ?? throw new ArgumentException($"Could not find a user with the specified user key ({userId})", nameof(userId));
        return FindAndPersistPastedTempImagesAsync(html, mediaParentFolder, user.Key).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Used by the RTE (and grid RTE) for drag/drop/persisting images.
    /// </summary>
    public async Task<string> FindAndPersistPastedTempImagesAsync(string html, Guid mediaParentFolder, Guid userKey)
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

        // An array to contain a list of URLs that
        // we have already processed to avoid dupes
        var uploadedImages = new Dictionary<Guid, GuidUdi>();

        foreach (HtmlNode? img in tmpImages)
        {
            // The data attribute contains the key of the temporary file
            var tmpImgKey = img.GetAttributeValue(TemporaryImageDataAttribute, string.Empty);
            if (Guid.TryParse(tmpImgKey, out Guid temporaryFileKey) is false)
            {
                continue;
            }

            TemporaryFileModel? temporaryFile = _temporaryFileService.GetAsync(temporaryFileKey).GetAwaiter().GetResult();
            if (temporaryFile is null)
            {
                continue;
            }

            GuidUdi udi;

            using (IScope scope = _scopeProvider.CreateScope())
            {
                _temporaryFileService.EnlistDeleteIfScopeCompletes(temporaryFileKey, _scopeProvider);

                if (uploadedImages.ContainsKey(temporaryFileKey) == false)
                {
                    var mediaTypeAlias = MediaTypeAlias(temporaryFile.FileName);

                    using Stream fileStream = temporaryFile.OpenReadStream();
                    Guid? parentFolderKey = mediaParentFolder == Guid.Empty ? Constants.System.RootKey : mediaParentFolder;
                    IMedia mediaFile = await _mediaImportService.ImportAsync(temporaryFile.FileName, fileStream, parentFolderKey, mediaTypeAlias, userKey);
                    udi = mediaFile.GetUdi();
                }
                else
                {
                    // Already been uploaded & we have it's UDI
                    udi = uploadedImages[temporaryFileKey];
                }

                scope.Complete();
            }

            UpdateImageNode(img, udi);

            // Remove the data attribute (so we do not re-process this)
            img.Attributes.Remove(TemporaryImageDataAttribute);

            // Add to the dictionary to avoid dupes
            uploadedImages.TryAdd(temporaryFileKey, udi);
        }

        return htmlDoc.DocumentNode.OuterHtml;
    }

    private string MediaTypeAlias(string fileName)
        => fileName.EndsWith(".svg")
            ? Constants.Conventions.MediaTypes.VectorGraphicsAlias
            : Constants.Conventions.MediaTypes.Image;

    private void UpdateImageNode(HtmlNode img, GuidUdi udi)
    {
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
            location = _imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(location)
            {
                ImageCropMode = ImageCropMode.Max,
                Width = width,
                Height = height,
            });
        }

        img.SetAttributeValue("src", location);
    }
}
