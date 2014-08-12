using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core
{
    public static class ContentBaseExtensions
    {
        public static void SanitizeEntityPropertiesForXmlStorage(this IContentBase entity)
        {
            entity.Name = entity.Name.ToValidXmlString();
            foreach (var property in entity.Properties)
            {
                if (property.Value is string)
                {
                    var value = (string)property.Value;
                    property.Value = value.ToValidXmlString();
                }
            }
        }
        
        public static void SanitizeTagsForXmlStorage(this ITag entity)
        {
            entity.Text = entity.Text.ToValidXmlString();
        }
    }
}
