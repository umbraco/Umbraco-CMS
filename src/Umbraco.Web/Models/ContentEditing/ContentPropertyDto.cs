using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a content property from the database
    /// </summary>
    internal class ContentPropertyDto : ContentPropertyBase
    {
        public IDataTypeDefinition DataType { get; set; }
        public string Label { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }        
    }
}