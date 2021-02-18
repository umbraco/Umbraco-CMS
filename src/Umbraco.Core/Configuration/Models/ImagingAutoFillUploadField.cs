// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for image autofill upload settings.
    /// </summary>
    public class ImagingAutoFillUploadField : ValidatableEntryBase
    {
        /// <summary>
        /// Gets or sets a value for the alias of the image upload property.
        /// </summary>
        [Required]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets a value for the width field alias of the image upload property.
        /// </summary>
        [Required]
        public string WidthFieldAlias { get; set; }

        /// <summary>
        /// Gets or sets a value for the height field alias of the image upload property.
        /// </summary>
        [Required]
        public string HeightFieldAlias { get; set; }

        /// <summary>
        /// Gets or sets a value for the length field alias of the image upload property.
        /// </summary>
        [Required]
        public string LengthFieldAlias { get; set; }

        /// <summary>
        /// Gets or sets a value for the extension field alias of the image upload property.
        /// </summary>
        [Required]
        public string ExtensionFieldAlias { get; set; }
    }
}
