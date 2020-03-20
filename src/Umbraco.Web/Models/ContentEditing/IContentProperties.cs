using System.Collections.Generic;

namespace Umbraco.Web.Models.ContentEditing
{

    public interface IContentProperties<T>
        where T : ContentPropertyBasic
    {
        IEnumerable<T> Properties { get; }
    }
}
