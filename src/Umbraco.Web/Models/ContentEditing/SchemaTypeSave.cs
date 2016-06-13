using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Model used to save a media type
    /// </summary>
    [DataContract(Name = "schemaType", Namespace = "")]
    public class SchemaTypeSave : ContentTypeSave<PropertyTypeBasic>
    {
    }
}