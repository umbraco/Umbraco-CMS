using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Web
{
    internal static class MediaPropertyExtensions
    {

        internal static void AutoPopulateFileMetaDataProperties(this IContentBase model, string propertyAlias, string relativefilePath = null)
        {
            var mediaFileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var uploadFieldConfigNode =
                   UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties
                                       .FirstOrDefault(x => x.Alias == propertyAlias);

            if (uploadFieldConfigNode != null && model.Properties.Contains(propertyAlias))
            {
                if (relativefilePath == null)
                    relativefilePath = model.GetValue<string>(propertyAlias);

                //now we need to check if there is a path
                if (!string.IsNullOrEmpty(relativefilePath))
                {
                    var fullPath = mediaFileSystem.GetFullPath(mediaFileSystem.GetRelativePath(relativefilePath));
                    var umbracoFile = new UmbracoMediaFile(fullPath);
                    FillProperties(uploadFieldConfigNode, model, umbracoFile);
                }
                else
                {
                    //for now I'm just resetting this
                    ResetProperties(uploadFieldConfigNode, model);
                }
            }
        }

        internal static void ResetFileMetaDataProperties(this IContentBase content, IImagingAutoFillUploadField uploadFieldConfigNode)
        {
            if (uploadFieldConfigNode == null) throw new ArgumentNullException("uploadFieldConfigNode");
            ResetProperties(uploadFieldConfigNode, content);
        }

        private static void ResetProperties(IImagingAutoFillUploadField uploadFieldConfigNode, IContentBase content)
        {
            if (content.Properties.Contains(uploadFieldConfigNode.WidthFieldAlias))
                content.Properties[uploadFieldConfigNode.WidthFieldAlias].Value = string.Empty;

            if (content.Properties.Contains(uploadFieldConfigNode.HeightFieldAlias))
                content.Properties[uploadFieldConfigNode.HeightFieldAlias].Value = string.Empty;

            if (content.Properties.Contains(uploadFieldConfigNode.LengthFieldAlias))
                content.Properties[uploadFieldConfigNode.LengthFieldAlias].Value = string.Empty;

            if (content.Properties.Contains(uploadFieldConfigNode.ExtensionFieldAlias))
                content.Properties[uploadFieldConfigNode.ExtensionFieldAlias].Value = string.Empty;
        }


        internal static void PopulateFileMetaDataProperties(this IContentBase content, IImagingAutoFillUploadField uploadFieldConfigNode, string relativeFilePath)
        {
            if (uploadFieldConfigNode == null) throw new ArgumentNullException("uploadFieldConfigNode");
            if (relativeFilePath.IsNullOrWhiteSpace() == false)
            {
                var mediaFileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
                var fullPath = mediaFileSystem.GetFullPath(mediaFileSystem.GetRelativePath(relativeFilePath));
                var umbracoFile = new UmbracoMediaFile(fullPath);
                FillProperties(uploadFieldConfigNode, content, umbracoFile);
            }
            else
            {
                //for now I'm just resetting this since we cant detect a file
                ResetProperties(uploadFieldConfigNode, content);
            }
        }

        private static void FillProperties(IImagingAutoFillUploadField uploadFieldConfigNode, IContentBase content, UmbracoMediaFile um)
        {
            if (uploadFieldConfigNode == null) throw new ArgumentNullException("uploadFieldConfigNode");
            if (content == null) throw new ArgumentNullException("content");
            if (um == null) throw new ArgumentNullException("um");
            var size = um.SupportsResizing ? (Size?)um.GetDimensions() : null;

            if (content.Properties.Contains(uploadFieldConfigNode.WidthFieldAlias))
                content.Properties[uploadFieldConfigNode.WidthFieldAlias].Value = size.HasValue ? size.Value.Width.ToInvariantString() : string.Empty;

            if (content.Properties.Contains(uploadFieldConfigNode.HeightFieldAlias))
                content.Properties[uploadFieldConfigNode.HeightFieldAlias].Value = size.HasValue ? size.Value.Height.ToInvariantString() : string.Empty;

            if (content.Properties.Contains(uploadFieldConfigNode.LengthFieldAlias))
                content.Properties[uploadFieldConfigNode.LengthFieldAlias].Value = um.Length;

            if (content.Properties.Contains(uploadFieldConfigNode.ExtensionFieldAlias))
                content.Properties[uploadFieldConfigNode.ExtensionFieldAlias].Value = um.Extension;
        }


    }
}
