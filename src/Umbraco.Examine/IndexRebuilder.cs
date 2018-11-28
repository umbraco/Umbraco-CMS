using System.Collections.Generic;
using System.Linq;
using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Utility to rebuild all indexes ensuring minimal data queries
    /// </summary>
    public class IndexRebuilder
    {
        private readonly IEnumerable<IIndexPopulator> _populators;
        public IExamineManager ExamineManager { get; }

        public IndexRebuilder(IExamineManager examineManager, IEnumerable<IIndexPopulator> populators)
        {
            _populators = populators;
            ExamineManager = examineManager;
        }

        public void RebuildIndexes(bool onlyEmptyIndexes)
        {
            var indexes = (onlyEmptyIndexes
                ? ExamineManager.IndexProviders.Values.Where(x => !x.IndexExists())
                : ExamineManager.IndexProviders.Values).ToArray();

            foreach(var index in indexes)
                index.CreateIndex(); // clear the index

            foreach (var populator in _populators)
            {
                populator.Populate(indexes);
            }   
        }
    }
}
