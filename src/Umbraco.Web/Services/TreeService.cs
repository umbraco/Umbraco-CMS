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
        public IEnumerable<Tree> GetAll() => _treeCollection;

        /// <inheritdoc />
        public IEnumerable<Tree> GetBySection(string sectionAlias)
            => _treeCollection.Where(x => x.SectionAlias.InvariantEquals(sectionAlias)).OrderBy(x => x.SortOrder).ToList();

        /// <inheritdoc />
        public IDictionary<string, IEnumerable<Tree>> GetBySectionGrouped(string sectionAlias)
        {
            return GetBySection(sectionAlias).GroupBy(x => x.TreeGroup).ToDictionary(
                x => x.Key ?? "",
                x => (IEnumerable<Tree>) x.ToArray());
        }
    }
}
