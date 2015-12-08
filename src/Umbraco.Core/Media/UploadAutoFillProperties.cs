using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Media
{
    /// <summary>
    /// Provides extension methods to manage auto-fill properties for upload fields.
    /// </summary>
    internal static class UploadAutoFillProperties
    {
        /// <summary>
        /// Gets the auto-fill configuration for a specified property alias.
        /// </summary>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <returns>The auto-fill configuration for the specified property alias, or null.</returns>
        public static IImagingAutoFillUploadField GetConfig(string propertyTypeAlias)
        {
            var autoFillConfigs = UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties;            
            return autoFillConfigs == null ? null : autoFillConfigs.FirstOrDefault(x => x.Alias == propertyTypeAlias);
        }

        /// <summary>
        /// Resets the auto-fill properties of a content item, for a specified property alias.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        public static void Reset(IContentBase content, string propertyTypeAlias)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (propertyTypeAlias == null) throw new ArgumentNullException("propertyTypeAlias");

            // get the config, no config = nothing to do
            var autoFillConfig = GetConfig(propertyTypeAlias);
            if (autoFillConfig == null) return; // nothing

            // reset
            Reset(content, autoFillConfig);
        }

        /// <summary>
        /// Resets the auto-fill properties of a content item, for a specified auto-fill configuration.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="autoFillConfig">The auto-fill configuration.</param>
        public static void Reset(IContentBase content, IImagingAutoFillUploadField autoFillConfig)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (autoFillConfig == null) throw new ArgumentNullException("autoFillConfig");

            ResetProperties(content, autoFillConfig);
        }

        /// <summary>
        /// Populates the auto-fill properties of a content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <param name="filepath">The filesystem-relative filepath, or null to clear properties.</param>
        public static void Populate(IContentBase content, string propertyTypeAlias, string filepath)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (propertyTypeAlias == null) throw new ArgumentNullException("propertyTypeAlias");

            // no property = nothing to do
            if (content.Properties.Contains(propertyTypeAlias) == false) return;

            // get the config, no config = nothing to do
            var autoFillConfig = GetConfig(propertyTypeAlias);
            if (autoFillConfig == null) return; // nothing

            // populate
            Populate(content, autoFillConfig, filepath);
        }

        /// <summary>
        /// Populates the auto-fill properties of a content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <param name="filepath">The filesystem-relative filepath, or null to clear properties.</param>
        /// <param name="filestream">The stream containing the file data.</param>
        /// <param name="image">The file data as an image object.</param>
        public static void Populate(IContentBase content, string propertyTypeAlias, string filepath, Stream filestream, Image image = null)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (propertyTypeAlias == null) throw new ArgumentNullException("propertyTypeAlias");

            // no property = nothing to do
            if (content.Properties.Contains(propertyTypeAlias) == false) return;

            // get the config, no config = nothing to do
            var autoFillConfig = GetConfig(propertyTypeAlias);
            if (autoFillConfig == null) return; // nothing

            // populate
            Populate(content, autoFillConfig, filepath, filestream, image);
        }

        /// <summary>
        /// Populates the auto-fill properties of a content item, for a specified auto-fill configuration.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="autoFillConfig">The auto-fill configuration.</param>
        /// <param name="filepath">The filesystem path to the uploaded file.</param>
        /// <remarks>The <paramref name="filepath"/> parameter is the path relative to the filesystem.</remarks>
        public static void Populate(IContentBase content, IImagingAutoFillUploadField autoFillConfig, string filepath)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (autoFillConfig == null) throw new ArgumentNullException("autoFillConfig");

            // no file = reset, file = auto-fill
            if (filepath.IsNullOrWhiteSpace())
            {
                ResetProperties(content, autoFillConfig);
            }
            else
            {
                var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
                using (var filestream = fs.OpenFile(filepath))
                {
                    var extension = (Path.GetExtension(filepath) ?? "").TrimStart('.');
                    var size = ImageHelper.IsImageFile(extension) ? (Size?) ImageHelper.GetDimensions(filestream) : null;
                    SetProperties(content, autoFillConfig, size, filestream.Length, extension);
                }
            }
        }

        /// <summary>
        /// Populates the auto-fill properties of a content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="autoFillConfig"></param>
        /// <param name="filepath">The filesystem-relative filepath, or null to clear properties.</param>
        /// <param name="filestream">The stream containing the file data.</param>
        /// <param name="image">The file data as an image object.</param>
        public static void Populate(IContentBase content, IImagingAutoFillUploadField autoFillConfig, string filepath, Stream filestream, Image image = null)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (autoFillConfig == null) throw new ArgumentNullException("autoFillConfig");

            // no file = reset, file = auto-fill
            if (filepath.IsNullOrWhiteSpace() || filestream == null)
            {
                ResetProperties(content, autoFillConfig);
            }
            else
            {
                var extension = (Path.GetExtension(filepath) ?? "").TrimStart('.');
                Size? size;
                if (image == null)
                    size = ImageHelper.IsImageFile(extension) ? (Size?) ImageHelper.GetDimensions(filestream) : null;
                else 
                    size = new Size(image.Width, image.Height);
                SetProperties(content, autoFillConfig, size, filestream.Length, extension);
            }
        }

        private static void SetProperties(IContentBase content, IImagingAutoFillUploadField autoFillConfig, Size? size, long length, string extension)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (autoFillConfig == null) throw new ArgumentNullException("autoFillConfig");

            if (content.Properties.Contains(autoFillConfig.WidthFieldAlias))
                content.Properties[autoFillConfig.WidthFieldAlias].Value = size.HasValue ? size.Value.Width.ToInvariantString() : string.Empty;

            if (content.Properties.Contains(autoFillConfig.HeightFieldAlias))
                content.Properties[autoFillConfig.HeightFieldAlias].Value = size.HasValue ? size.Value.Height.ToInvariantString() : string.Empty;

            if (content.Properties.Contains(autoFillConfig.LengthFieldAlias))
                content.Properties[autoFillConfig.LengthFieldAlias].Value = length;

            if (content.Properties.Contains(autoFillConfig.ExtensionFieldAlias))
                content.Properties[autoFillConfig.ExtensionFieldAlias].Value = extension;
}

        private static void ResetProperties(IContentBase content, IImagingAutoFillUploadField autoFillConfig)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (autoFillConfig == null) throw new ArgumentNullException("autoFillConfig");

            if (content.Properties.Contains(autoFillConfig.WidthFieldAlias))
                content.Properties[autoFillConfig.WidthFieldAlias].Value = string.Empty;

            if (content.Properties.Contains(autoFillConfig.HeightFieldAlias))
                content.Properties[autoFillConfig.HeightFieldAlias].Value = string.Empty;

            if (content.Properties.Contains(autoFillConfig.LengthFieldAlias))
                content.Properties[autoFillConfig.LengthFieldAlias].Value = string.Empty;

            if (content.Properties.Contains(autoFillConfig.ExtensionFieldAlias))
                content.Properties[autoFillConfig.ExtensionFieldAlias].Value = string.Empty;
        }
    }
}
