using System.Collections.Generic;

namespace Umbraco.Web.Models.ContentEditing
{

    public interface ITabbedContent<T>
        where T : ContentPropertyBasic
    {
        IEnumerable<Tab<T>> Tabs { get; }
    }
}
