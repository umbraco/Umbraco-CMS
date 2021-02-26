// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the image cropper value editor.
    /// </summary>
    public class ImageCropperConfiguration
    {
        [ConfigurationField("crops", "Define crops", "views/propertyeditors/imagecropper/imagecropper.prevalues.html")]
        public Crop[] Crops { get; set; }

        [DataContract]
        public class Crop
        {
            [DataMember(Name = "alias")]
            public string Alias { get; set; }

            [DataMember(Name = "width")]
            public int Width { get; set; }

            [DataMember(Name = "height")]
            public int Height { get; set; }
        }
    }
}
