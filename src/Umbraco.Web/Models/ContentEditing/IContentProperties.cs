using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{

    public interface IContentProperties<T>
        where T : ContentPropertyBasic
    {
        IEnumerable<T> Properties { get; }
    }
}
