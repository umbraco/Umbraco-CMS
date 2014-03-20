using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing a media item to be saved
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class MediaItemSave : ContentBaseItemSave<IMedia>
    {

    }
}