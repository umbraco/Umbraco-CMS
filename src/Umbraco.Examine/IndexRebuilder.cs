using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Examine;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Umbraco.Examine
{   

    /// <summary>
    /// Utility to rebuild all indexes ensuring minimal data queries
    /// </summary>
    public class IndexRebuilder
    {
        private readonly IProfilingLogger _logger;
        private readonly IEnumerable<IIndexPopulator> _populators;
        public IExamineManager ExamineManager { get; }

        [Obsolete("Use constructor with all dependencies")]
        public IndexRebuilder(IExamineManager examineManager, IEnumerable<IIndexPopulator> populators)
            : this(Current.ProfilingLogger, examineManager, populators)
        {
        }

        public IndexRebuilder(IProfilingLogger logger, IExamineManager examineManager, IEnumerable<IIndexPopulator> populators)
        {
            _populators = populators;
            _logger = logger;
            ExamineManager = examineManager;
        }

        public bool CanRebuild(IIndex index)
        {
            return _populators.Any(x => x.IsRegistered(index));
        }

        public void RebuildIndex(string indexName)
        {
            if (!ExamineManager.TryGetIndex(indexName, out var index))
                throw new InvalidOperationException($"No index found with name {indexName}");
            index.CreateIndex(); // clear the index
            foreach (var populator in _populators)
            {
                populator.Populate(index);
            }
        }

        public void RebuildIndexes(bool onlyEmptyIndexes)
        {
            var indexes = (onlyEmptyIndexes
                ? ExamineManager.Indexes.Where(x => !x.IndexExists())
                : ExamineManager.Indexes).ToArray();

            if (indexes.Length == 0) return;

            foreach (var index in indexes)
            {
                index.CreateIndex(); // clear the index
            }

            // run each populator over the indexes
            foreach(var populator in _populators)
            {
                try
                {
                    populator.Populate(indexes);
                }
                catch (Exception e)
                {
                    _logger.Error<IndexRebuilder,Type>(e, "Index populating failed for populator {Populator}", populator.GetType());                    
                }
            }
        }

    }
}
