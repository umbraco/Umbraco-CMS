using Umbraco.Cms.Core.Trees;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements <see cref="ITreeService" />.
/// </summary>
public class TreeService : ITreeService
{
    private readonly TreeCollection _treeCollection;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TreeService" /> class.
    /// </summary>
    /// <param name="treeCollection"></param>
    public TreeService(TreeCollection treeCollection) => _treeCollection = treeCollection;

    /// <inheritdoc />
    public Tree? GetByAlias(string treeAlias) => _treeCollection.FirstOrDefault(x => x.TreeAlias == treeAlias);

    /// <inheritdoc />
    public IEnumerable<Tree> GetAll(TreeUse use = TreeUse.Main)

        // use HasFlagAny: if use is Main|Dialog, we want to return Main *and* Dialog trees
        => _treeCollection.Where(x => x.TreeUse.HasFlagAny(use));

    /// <inheritdoc />
    public IEnumerable<Tree> GetBySection(string sectionAlias, TreeUse use = TreeUse.Main)

        // use HasFlagAny: if use is Main|Dialog, we want to return Main *and* Dialog trees
        => _treeCollection.Where(x => x.SectionAlias.InvariantEquals(sectionAlias) && x.TreeUse.HasFlagAny(use))
            .OrderBy(x => x.SortOrder).ToList();

    /// <inheritdoc />
    public IDictionary<string, IEnumerable<Tree>>
        GetBySectionGrouped(string sectionAlias, TreeUse use = TreeUse.Main) =>
        GetBySection(sectionAlias, use).GroupBy(x => x.TreeGroup).ToDictionary(
            x => x.Key ?? string.Empty,
            x => (IEnumerable<Tree>)x.ToArray());
}
