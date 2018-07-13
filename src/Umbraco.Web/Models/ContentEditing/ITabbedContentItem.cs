using System.Collections.Generic;

namespace Umbraco.Web.Models.ContentEditing
{
    public interface ITabbedContentItem<T> where T : ContentPropertyBasic
    {
        IEnumerable<T> Properties { get; set; }
        IEnumerable<Tab<T>> Tabs { get; set; }
    }
}
