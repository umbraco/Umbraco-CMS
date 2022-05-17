namespace Umbraco.Cms.Core.Models.ContentEditing;

public interface ITabbedContent<T>
    where T : ContentPropertyBasic
{
    IEnumerable<Tab<T>> Tabs { get; }
}
