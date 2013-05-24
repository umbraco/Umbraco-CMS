using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [CollectionDataContract(Name = "properties", Namespace = "")]
    public class ContentPropertyCollection<T> : List<T>
        where T : ContentPropertyBase
    {        
    }
}