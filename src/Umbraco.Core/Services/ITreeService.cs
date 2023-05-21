using Umbraco.Cms.Core.Trees;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents a service which manages section trees.
/// </summary>
public interface ITreeService
{
    /// <summary>
    ///     Gets a tree.
    /// </summary>
    /// <param name="treeAlias">The tree alias.</param>
    Tree? GetByAlias(string treeAlias);

    /// <summary>
    ///     Gets all trees.
    /// </summary>
    IEnumerable<Tree> GetAll(TreeUse use = TreeUse.Main);

    /// <summary>
    ///     Gets all trees for a section.
    /// </summary>
    IEnumerable<Tree> GetBySection(string sectionAlias, TreeUse use = TreeUse.Main);

    /// <summary>
    ///     Gets all trees for a section, grouped.
    /// </summary>
    IDictionary<string, IEnumerable<Tree>> GetBySectionGrouped(string sectionAlias, TreeUse use = TreeUse.Main);
}
