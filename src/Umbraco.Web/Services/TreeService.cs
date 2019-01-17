using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Services
{
    internal class TreeService : ITreeService
    {
        private readonly TreeCollection _treeCollection;
        private readonly Lazy<IReadOnlyCollection<IGrouping<string, string>>> _groupedTrees;

        public TreeService(TreeCollection treeCollection)
        {
            _treeCollection = treeCollection;
            _groupedTrees = new Lazy<IReadOnlyCollection<IGrouping<string, string>>>(InitGroupedTrees);
        }

        /// <inheritdoc />
        public Tree GetByAlias(string treeAlias) => _treeCollection.FirstOrDefault(t => t.TreeAlias == treeAlias);

        /// <inheritdoc />
        public IEnumerable<Tree> GetAll() => _treeCollection;

        /// <inheritdoc />
        public IEnumerable<Tree> GetTrees(string sectionAlias)
            => GetAll().Where(x => x.ApplicationAlias.InvariantEquals(sectionAlias)).OrderBy(x => x.SortOrder).ToList();

        public IDictionary<string, IEnumerable<Tree>> GetGroupedTrees(string sectionAlias)
        {
            var result = new Dictionary<string, IEnumerable<Tree>>();
            var foundTrees = GetTrees(sectionAlias).ToList();
            foreach(var treeGroup in _groupedTrees.Value)
            {
                List<Tree> resultGroup = null;
                foreach(var tree in foundTrees)
                { 
                    foreach(var treeAliasInGroup in treeGroup)
                    {
                        if (tree.TreeAlias != treeAliasInGroup) continue;

                        if (resultGroup == null) resultGroup = new List<Tree>();
                        resultGroup.Add(tree);
                    }  
                }
                if (resultGroup != null)
                    result[treeGroup.Key ?? string.Empty] = resultGroup; //key cannot be null so make empty string
            }
            return result;
        }

        /// <summary>
        /// Creates a group of all tree groups and their tree aliases
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Used to initialize the <see cref="_groupedTrees"/> field
        /// </remarks>
        private IReadOnlyCollection<IGrouping<string, string>> InitGroupedTrees()
        {
            var result = GetAll()
                .Select(x => (treeAlias: x.TreeAlias, treeGroup: x.TreeControllerType.GetCustomAttribute<CoreTreeAttribute>(false)?.TreeGroup))
                .GroupBy(x => x.treeGroup, x => x.treeAlias)
                .ToList();
            return result;
        }

    }
}
