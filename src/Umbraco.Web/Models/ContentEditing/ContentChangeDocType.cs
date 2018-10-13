using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.Models.ContentEditing
{
    public class ContentChangeDocType
    {
        /// <summary>
        /// Id of the current document
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Id of the new document type
        /// </summary>
        [Required]
        public int NewDocType { get; set; }

        /// <summary>
        /// Id of the new template type 
        /// </summary>
        [Required]
        public int NewTemplateType { get; set; }

        /// <summary>
        /// List of all property mappings set in window
        /// </summary>
        [Required]
        public IEnumerable<PropertyMapping> PropertyMappings { get; set; }

        /// <summary>
        /// Whther document chnage was successful
        /// </summary>
        public bool Success { get; set; }
    }

    public class PropertyMapping
    {
        public string FromName { get; set; }
        public string FromAlias { get; set; }
        public string ToName { get; set; }
        public string ToAlias { get; set; }
        public object Value { get; set; }
    }
}
