using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models;

public class ListViewPagedModel<TContent>
    where TContent : IContentBase
{
    public required PagedModel<TContent> Items { get; init; }

    public required ListViewConfiguration ListViewConfiguration { get; init; }
}
