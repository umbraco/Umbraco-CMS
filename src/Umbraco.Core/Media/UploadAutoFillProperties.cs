using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Core.Media
{
    /// <summary>
    /// Provides methods to manage auto-fill properties for upload fields.
    /// </summary>
    internal class UploadAutoFillProperties
    {
        private readonly ILogger _logger;
        private readonly MediaFileSystem _mediaFileSystem;
        private readonly IContentSection _contentSettings;

        public UploadAutoFillProperties(MediaFileSystem mediaFileSystem, ILogger logger, IContentSection contentSettings)
        {
            _mediaFileSystem = mediaFileSystem;
            _logger = logger;
            _contentSettings = contentSettings;
        }

        /// <summary>
        /// Gets the auto-fill configuration for a specified property alias.
        /// </summary>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <returns>The auto-fill configuration for the specified property alias, or null.</returns>
        public IImagingAutoFillUploadField GetConfig(string propertyTypeAlias)
        {
            var autoFillConfigs = _contentSettings.ImageAutoFillProperties;            
            return autoFillConfigs == null ? null : autoFillConfigs.FirstOrDefault(x => x.Alias == propertyTypeAlias);
        }

        /// <summary>
        /// Resets the auto-fill properties of a content item, for a specified property alias.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        public void Reset(IContentBase content, string propertyTypeAlias)
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
        public void Reset(IContentBase content, IImagingAutoFillUploadField autoFillConfig)
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
        public void Populate(IContentBase content, string propertyTypeAlias, string filepath)
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
        public void Populate(IContentBase content, string propertyTypeAlias, string filepath, Stream filestream)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (propertyTypeAlias == null) throw new ArgumentNullException("propertyTypeAlias");

            // no property = nothing to do
            if (content.Properties.Contains(propertyTypeAlias) == false) return;

            // get the config, no config = nothing to do
            var autoFillConfig = GetConfig(propertyTypeAlias);
            if (autoFillConfig == null) return; // nothing

            // populate
            Populate(content, autoFillConfig, filepath, filestream);
        }

        /// <summary>
        /// Populates the auto-fill properties of a content item, for a specified auto-fill configuration.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="autoFillConfig">The auto-fill configuration.</param>
        /// <param name="filepath">The filesystem path to the uploaded file.</param>
        /// <remarks>The <paramref name="filepath"/> parameter is the path relative to the filesystem.</remarks>
        public void Populate(IContentBase content, IImagingAutoFillUploadField autoFillConfig, string filepath)
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
                // if anything goes wrong, just reset the properties
                try
                {
                    using (var filestream = _mediaFileSystem.OpenFile(filepath))
                    {
                        var extension = (Path.GetExtension(filepath) ?? "").TrimStart('.');
                        var size = _mediaFileSystem.IsImageFile(extension) ? (Size?) _mediaFileSystem.GetDimensions(filestream) : null;
                        SetProperties(content, autoFillConfig, size, filestream.Length, extension);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(typeof(UploadAutoFillProperties), "Could not populate upload auto-fill properties for file \""
                        + filepath + "\".", ex);
                    ResetProperties(content, autoFillConfig);
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
        public void Populate(IContentBase content, IImagingAutoFillUploadField autoFillConfig, string filepath, Stream filestream)
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
                var size = _mediaFileSystem.IsImageFile(extension) ? (Size?)_mediaFileSystem.GetDimensions(filestream) : null;
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
