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

        public ContentPropertyDisplay ForDisplay(string getPreValue, string view)
        {
            return new ContentPropertyDisplay
                {
                    Alias = Alias,
                    Id = Id,
                    View = view,
                    Config = getPreValue,
                    Description = Description,
                    Label = Label,
                    Value = Value
                };
        }
    }
}