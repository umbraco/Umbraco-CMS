using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Services
{
    /// <summary>
    /// Implements <see cref="ITreeService"/>.
    /// </summary>
    internal class TreeService : ITreeService
    {
        private readonly TreeCollection _treeCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeService"/> class.
        /// </summary>
        /// <param name="treeCollection"></param>
        public TreeService(TreeCollection treeCollection)
        {
            _treeCollection = treeCollection;
        }

        /// <inheritdoc />
        public Tree GetByAlias(string treeAlias) => _treeCollection.FirstOrDefault(x => x.TreeAlias == treeAlias);

        /// <inheritdoc />
        public IEnumerable<Tree> GetAll(TreeUse use = TreeUse.Main)
            // use HasFlagAny: if use is Main|Dialog, we want to return Main *and* Dialog trees
            => _treeCollection.Where(x => x.TreeUse.HasFlagAny(use));

        /// <inheritdoc />
        public IEnumerable<Tree> GetBySection(string sectionAlias, TreeUse use = TreeUse.Main)
            // use HasFlagAny: if use is Main|Dialog, we want to return Main *and* Dialog trees
            => _treeCollection.Where(x => x.SectionAlias.InvariantEquals(sectionAlias) && x.TreeUse.HasFlagAny(use)).OrderBy(x => x.SortOrder).ToList();

        /// <inheritdoc />
        public IDictionary<string, IEnumerable<Tree>> GetBySectionGrouped(string sectionAlias, TreeUse use = TreeUse.Main)
        {
            return GetBySection(sectionAlias, use).GroupBy(x => x.TreeGroup).ToDictionary(
                x => x.Key ?? "",
                x => (IEnumerable<Tree>) x.ToArray());
        }
    }
}
