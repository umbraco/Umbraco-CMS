using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using Umbraco.Core;
using Umbraco.Core.Composing;
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
        public ContentTypeBasic()
        {
            Blueprints = new Dictionary<int, string>();
        }

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
        [ReadOnly(true)]
        public string Thumbnail
        {
            get
            {
                var thumbsFolder = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Thumbnails));
                var files = Directory.GetFiles(thumbsFolder.FullName, $"{Alias}.*");
                if (files.Length == 1) // ignore duplicates
                {
                    return IOHelper.ResolveVirtualUrl($"{SystemDirectories.Thumbnails}/{System.IO.Path.GetFileName(files[0])}");
                }

                return null;                
            }
        }

        [DataMember(Name = "blueprints")]
        [ReadOnly(true)]
        public IDictionary<int, string> Blueprints { get; set; }

        [DataMember(Name = "isContainer")]
        [ReadOnly(true)]
        public bool IsContainer { get; set; }

        [DataMember(Name = "isElement")]
        [ReadOnly(true)]
        public bool IsElement { get; set; }
    }
}
