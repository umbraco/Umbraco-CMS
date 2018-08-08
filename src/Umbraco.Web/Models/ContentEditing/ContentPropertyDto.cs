using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a content property from the database
    /// </summary>
    internal class ContentPropertyDto : ContentPropertyBasic
    {
        public IDataType DataType { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRegExp { get; set; }

        /// <summary>
        /// The culture for the property which is only relevant for variant properties
        /// </summary>
        /// <remarks>
        /// Since content currently is the only thing that can be variant this will only be relevant to content
        /// </remarks>
        public string Culture { get; set; }
    }
}
