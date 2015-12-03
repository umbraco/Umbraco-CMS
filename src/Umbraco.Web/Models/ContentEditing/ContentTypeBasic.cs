using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Validation;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A basic version of a content type
    /// </summary>
    /// <remarks>
    /// Generally used to return the minimal amount of data about a content type
    /// </remarks> 
    [DataContract(Name = "contentType", Namespace = "")]
    public class ContentTypeBasic : EntityBasic
    {
        /// <summary>
        /// Overridden to apply our own validation attributes since this is not always required for other classes
        /// </summary>        
        [Required]
        [RegularExpression(@"^([a-zA-Z]\w.*)$", ErrorMessage = "Invalid alias")]
        [DataMember(Name = "alias")]
        public override string Alias { get; set; }

        [DataMember(Name = "updateDate")]
        [ReadOnly(true)]
        public DateTime UpdateDate { get; set; }

        [DataMember(Name = "createDate")]
        [ReadOnly(true)]
        public DateTime CreateDate { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "thumbnail")]
        public string Thumbnail { get; set; }

        /// <summary>
        /// Returns true if the icon represents a CSS class instead of a file path
        /// </summary>
        [DataMember(Name = "iconIsClass")]
        [ReadOnly(true)]
        public bool IconIsClass
        {
            get
            {
                if (Icon.IsNullOrWhiteSpace())
                {
                    return true;
                }
                //if it starts with a '.' or doesn't contain a '.' at all then it is a class
                return Icon.StartsWith(".") || Icon.Contains(".") == false;
            }
        }

        /// <summary>
        /// Returns the icon file path if the icon is not a class, otherwise returns an empty string
        /// </summary>
        [DataMember(Name = "iconFilePath")]
        [ReadOnly(true)]
        public string IconFilePath
        {
            get
            {
                return IconIsClass
                    ? string.Empty
                    : string.Format("{0}images/umbraco/{1}", GlobalSettings.Path.EnsureEndsWith("/"), Icon);
            }
        }

        /// <summary>
        /// Returns true if the icon represents a CSS class instead of a file path
        /// </summary>
        [DataMember(Name = "thumbnailIsClass")]
        [ReadOnly(true)]
        public bool ThumbnailIsClass
        {
            get
            {
                if (Thumbnail.IsNullOrWhiteSpace())
                {
                    return true;
                }
                //if it starts with a '.' or doesn't contain a '.' at all then it is a class
                return Thumbnail.StartsWith(".") || Thumbnail.Contains(".") == false;
            }
        }

        /// <summary>
        /// Returns the icon file path if the icon is not a class, otherwise returns an empty string
        /// </summary>
        [DataMember(Name = "thumbnailFilePath")]
        [ReadOnly(true)]
        public string ThumbnailFilePath
        {
            get
            {
                return ThumbnailIsClass
                           ? string.Empty
                           : IOHelper.ResolveUrl("~/umbraco/images/thumbnails/" + Thumbnail);
            }
        }
    }
}
