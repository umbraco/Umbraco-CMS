namespace Umbraco.Core.Models
{
    /// <summary>
    /// The definition of a published property
    /// </summary>
    public class PublishedPropertyDefinition
    {
        public PublishedPropertyDefinition(string propertyTypeAlias, string documentTypeAlias, string propertyEditorAlias)
        {
            //PropertyId = propertyId;
            DocumentTypeAlias = documentTypeAlias;
            PropertyTypeAlias = propertyTypeAlias;
            PropertyEditorAlias = propertyEditorAlias;
            //ItemType = itemType;
        }

        //public int PropertyId { get; private set; }
        public string DocumentTypeAlias { get; private set; }
        public string PropertyTypeAlias { get; private set; }
        public string PropertyEditorAlias { get; private set; }
        //public PublishedItemType ItemType { get; private set; }
    }
}