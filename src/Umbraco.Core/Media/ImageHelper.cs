using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Media.Exif;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Media
{
    /// <summary>
    /// Provides helper methods for managing images.
    /// </summary>
    public class ImageHelper // fixme kill!
    {
        private readonly IContentSection _contentSection;

        public ImageHelper(IContentSection contentSection)
        {
            _contentSection = contentSection;
        }

        /// <summary>
        /// Gets a value indicating whether the file extension corresponds to an image.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>A value indicating whether the file extension corresponds to an image.</returns>
        public bool IsImageFile(string extension)
        {
            if (extension == null) return false;
            extension = extension.TrimStart('.');
            return _contentSection.ImageFileTypes.InvariantContains(extension);
        }
    }
}
