using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors
{
    public sealed class RichTextEditorPastedImages
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILogger _logger;
        private readonly IMediaService _mediaService;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly string _tempFolderAbsolutePath;

        // An array to contain a list of URLs that we have already processed to avoid dupes
        private static readonly Dictionary<string, GuidUdi> UploadedImages = new();

        const string TemporaryImageDataAttribute = "data-tmpimg";

        public RichTextEditorPastedImages(IUmbracoContextAccessor umbracoContextAccessor, ILogger logger, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider ?? throw new ArgumentNullException(nameof(contentTypeBaseServiceProvider));
            _tempFolderAbsolutePath = Path.GetFullPath(IOHelper.MapPath(SystemDirectories.TempImageUploads));
        }

        /// <summary>
        /// Used by the RTE (and grid RTE) for converting inline base64 images to Media items
        /// </summary>
        /// <param name="html"></param>
        /// <param name="mediaParentFolder"></param>
        /// <param name="userId"></param>
        /// <param name="imageUrlGenerator"></param>
        /// <returns></returns>
        internal string FindAndPersistBase64Images(string html, Guid mediaParentFolder, int userId, IImageUrlGenerator imageUrlGenerator)
        {
            // Find all img's that has data-tmpimg attribute
            // Use HTML Agility Pack - https://html-agility-pack.net
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var imagesWithDataUris = htmlDoc.DocumentNode.SelectNodes("//img");
            if (imagesWithDataUris == null || imagesWithDataUris.Count == 0)
                return html;

            foreach (var img in imagesWithDataUris)
            {
                var srcValue = img.GetAttributeValue("src", string.Empty);

                // Ignore src-less images
                if (string.IsNullOrEmpty(srcValue))
                    continue;

                // Take only images that have a "data:image" uri into consideration
                if (!srcValue.StartsWith("data:image"))
                    continue;

                // Create tmp image by scanning the srcValue
                // the value will look like "data:image/jpg;base64,abc" where the first part
                // is the mimetype and the second (after the comma) is the image blob
                var tokens = srcValue.Split(',');
                var dataUriInfo = tokens[0];
                var mimeType = dataUriInfo.Split(';')[0].Replace("data:", string.Empty);
                var base64imageString = tokens[1];

                // Create an unique folder path to help with concurrent users to avoid filename clash
                var imageTempPath = IOHelper.MapPath(SystemDirectories.TempImageUploads + IOHelper.DirSepChar + Guid.NewGuid().ToString());

                // Ensure image temp path exists
                if (Directory.Exists(imageTempPath) == false)
                    Directory.CreateDirectory(imageTempPath);

                // To get the filename, we simply manipulate the mimetype into a filename
                var filePath = mimeType.Replace('/', '.');
                var tmpImgPath = imageTempPath + IOHelper.DirSepChar + filePath;
                var absoluteTmpImgPath = IOHelper.MapPath(tmpImgPath);

                // Convert the base64 content to a byte array and save the bytes directly to a file
                // this method should work for most use-cases
                System.IO.File.WriteAllBytes(absoluteTmpImgPath, Convert.FromBase64String(base64imageString));

                // When the temp file has been created, we can persist it
                PersistMediaItem(mediaParentFolder, userId, img, tmpImgPath, imageUrlGenerator);
            }

            return htmlDoc.DocumentNode.OuterHtml;
        }

        /// <summary>
        /// Used by the RTE (and grid RTE) for drag/drop/persisting images
        /// </summary>
        /// <param name="html"></param>
        /// <param name="mediaParentFolder"></param>
        /// <param name="userId"></param>
        /// <param name="imageUrlGenerator"></param>
        /// <returns></returns>
        internal string FindAndPersistPastedTempImages(string html, Guid mediaParentFolder, int userId, IImageUrlGenerator imageUrlGenerator)
        {
            // Find all img's that has data-tmpimg attribute
            // Use HTML Agility Pack - https://html-agility-pack.net
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var tmpImages = htmlDoc.DocumentNode.SelectNodes($"//img[@{TemporaryImageDataAttribute}]");
            if (tmpImages == null || tmpImages.Count == 0)
                return html;

            foreach (var img in tmpImages)
            {
                // The data attribute contains the path to the tmp img to persist as a media item
                var tmpImgPath = img.GetAttributeValue(TemporaryImageDataAttribute, string.Empty);

                if (string.IsNullOrEmpty(tmpImgPath))
                    continue;

                PersistMediaItem(mediaParentFolder, userId, img, tmpImgPath, imageUrlGenerator);
            }

            return htmlDoc.DocumentNode.OuterHtml;
        }

        private void PersistMediaItem(Guid mediaParentFolder, int userId, HtmlNode img, string tmpImgPath, IImageUrlGenerator imageUrlGenerator)
        {
            var absoluteTempImagePath = Path.GetFullPath(IOHelper.MapPath(tmpImgPath));

                if (IsValidPath(absoluteTempImagePath) == false)
                {
                    continue;
                }

            var fileName = Path.GetFileName(absoluteTempImagePath);
            var safeFileName = fileName.ToSafeFileName();

            var mediaItemName = safeFileName.ToFriendlyName();
            IMedia mediaFile;
            GuidUdi udi;

            if (UploadedImages.ContainsKey(tmpImgPath) == false)
            {
                if (mediaParentFolder == Guid.Empty)
                    mediaFile = _mediaService.CreateMedia(mediaItemName, Constants.System.Root, Constants.Conventions.MediaTypes.Image, userId);
                else
                    mediaFile = _mediaService.CreateMedia(mediaItemName, mediaParentFolder, Constants.Conventions.MediaTypes.Image, userId);

                var fileInfo = new FileInfo(absoluteTempImagePath);

                var fileStream = fileInfo.OpenReadWithRetry();
                if (fileStream == null) throw new InvalidOperationException("Could not acquire file stream");
                using (fileStream)
                {
                    mediaFile.SetValue(_contentTypeBaseServiceProvider, Constants.Conventions.Media.File, safeFileName, fileStream);
                }

                _mediaService.Save(mediaFile, userId);

                udi = mediaFile.GetUdi();
            }
            else
            {
                // Already been uploaded & we have it's UDI
                udi = UploadedImages[tmpImgPath];
            }

            // Add the UDI to the img element as new data attribute
            img.SetAttributeValue("data-udi", udi.ToString());

            // Get the new persisted image URL
            var mediaTyped = _umbracoContextAccessor?.UmbracoContext?.Media.GetById(udi.Guid);
            if (mediaTyped == null)
                throw new PanicException($"Could not find media by id {udi.Guid} or there was no UmbracoContext available.");

            var location = mediaTyped.Url();

            // Find the width & height attributes as we need to set the imageprocessor QueryString
            var width = img.GetAttributeValue("width", int.MinValue);
            var height = img.GetAttributeValue("height", int.MinValue);

            if (width != int.MinValue && height != int.MinValue)
            {
                location = imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(location) { ImageCropMode = "max", Width = width, Height = height });
            }

            img.SetAttributeValue("src", location);

            // Remove the data attribute (so we do not re-process this)
            img.Attributes.Remove(TemporaryImageDataAttribute);

            // Add to the dictionary to avoid dupes
            if (UploadedImages.ContainsKey(tmpImgPath) == false)
            {
                UploadedImages.Add(tmpImgPath, udi);

                // Delete folder & image now its saved in media
                // The folder should contain one image - as a unique guid folder created
                // for each image uploaded from TinyMceController
                var folderName = Path.GetDirectoryName(absoluteTempImagePath);
                try
                {
                    Directory.Delete(folderName, true);
                }
                catch (Exception ex)
                {
                    _logger.Error<string>(typeof(HtmlImageSourceParser), ex, "Could not delete temp file or folder {FileName}", absoluteTempImagePath);
                }
            }
        }

        private bool IsValidPath(string imagePath)
        {
            return imagePath.StartsWith(_tempFolderAbsolutePath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
