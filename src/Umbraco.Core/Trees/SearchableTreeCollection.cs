using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Trees;

public class SearchableTreeCollection : BuilderCollectionBase<ISearchableTree>
{
    private readonly Dictionary<string, SearchableApplicationTree> _dictionary;

    public SearchableTreeCollection(Func<IEnumerable<ISearchableTree>> items, ITreeService treeService)
        : base(items) =>
        _dictionary = CreateDictionary(treeService);

    public IReadOnlyDictionary<string, SearchableApplicationTree> SearchableApplicationTrees => _dictionary;

    public SearchableApplicationTree this[string key] => _dictionary[key];

    private Dictionary<string, SearchableApplicationTree> CreateDictionary(ITreeService treeService)
    {
        Tree[] appTrees = treeService.GetAll()
            .OrderBy(x => x.SortOrder)
            .ToArray();
        var dictionary = new Dictionary<string, SearchableApplicationTree>(StringComparer.OrdinalIgnoreCase);
        ISearchableTree[] searchableTrees = this.ToArray();
        foreach (Tree appTree in appTrees)
        {
            ISearchableTree? found = searchableTrees.FirstOrDefault(x => x.TreeAlias.InvariantEquals(appTree.TreeAlias));
            if (found != null)
            {
                SearchableTreeAttribute? searchableTreeAttribute =
                    found.GetType().GetCustomAttribute<SearchableTreeAttribute>(false);
                dictionary[found.TreeAlias] = new SearchableApplicationTree(
                    appTree.SectionAlias,
                    appTree.TreeAlias,
                    searchableTreeAttribute?.SortOrder ?? SearchableTreeAttribute.DefaultSortOrder,
                    searchableTreeAttribute?.ServiceName ?? string.Empty,
                    searchableTreeAttribute?.MethodName ?? string.Empty,
                    found);
            }
        }

        return dictionary;
    }
}
