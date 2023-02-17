using Examine;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Search;

public class UmbracoExamineIndex<T> : IUmbracoIndex<T>
{
    private readonly IIndex _examineIndex;

    public UmbracoExamineIndex(IIndex examineIndex)
    {
        _examineIndex = examineIndex;
    }

    public bool EnableDefaultEventHandler { get; }
    public bool PublishedValuesOnly { get; }
    public void IndexItems<T>(T[] members) => throw new NotImplementedException();
}
