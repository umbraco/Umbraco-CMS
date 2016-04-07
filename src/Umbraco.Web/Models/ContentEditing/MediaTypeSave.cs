using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Model used to save a media type
    /// </summary>
    [DataContract(Name = "contentType", Namespace = "")]
    public class MediaTypeSave : ContentTypeSave<PropertyTypeBasic>
    {
    }
}