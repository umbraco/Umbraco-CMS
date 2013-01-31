using System;

namespace Umbraco.Tests.CodeFirst.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ContentTypeAttribute : Attribute
    {
        public ContentTypeAttribute(string @alias)
        {
            Alias = alias;

            IconUrl = "folder.gif";
            Thumbnail = "folder.png";
            Description = "";
        }

        /// <summary>
        /// Gets or sets the Alias of the ContentType
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets or sets the optional Name of the ContentType
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the optional Description of the ContentType
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the optional IconUrl of the ContentType
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Gets or sets the optional Thumbnail of the ContentType
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// Gets or sets the optional array of Allowed Child ContentTypes of the ContentType
        /// </summary>
        public Type[] AllowedChildContentTypes { get; set; }

        /// <summary>
        /// Gets or sets the optional array of Allowed Template names of the ContentType
        /// </summary>
        public string[] AllowedTemplates { get; set; }
    }
}