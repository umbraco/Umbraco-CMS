using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a content property from the database
    /// </summary>
    internal class ContentPropertyDto : ContentPropertyBasic
    {
        public IDataTypeDefinition DataType { get; set; }
        public string Label { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public PropertyEditor PropertyEditor { get; set; }
    }
}