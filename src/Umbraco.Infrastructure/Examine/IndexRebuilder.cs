using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Examine;
using Umbraco.Cms.Core.Logging;
using Umbraco.Core.Logging;

namespace Umbraco.Examine
{

    /// <summary>
    /// Utility to rebuild all indexes ensuring minimal data queries
    /// </summary>
    public class IndexRebuilder
    {
        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<IndexRebuilder> _logger;
        private readonly IEnumerable<IIndexPopulator> _populators;
        public IExamineManager ExamineManager { get; }

        public IndexRebuilder(IProfilingLogger profilingLogger , ILogger<IndexRebuilder> logger, IExamineManager examineManager, IEnumerable<IIndexPopulator> populators)
        {
            _profilingLogger = profilingLogger ;
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

            OnRebuildingIndexes(new IndexRebuildingEventArgs(indexes));

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
                    _logger.LogError(e, "Index populating failed for populator {Populator}", populator.GetType());
                }
            }
        }

        /// <summary>
        /// Event raised when indexes are being rebuilt
        /// </summary>
        public event EventHandler<IndexRebuildingEventArgs> RebuildingIndexes;

        private void OnRebuildingIndexes(IndexRebuildingEventArgs args) => RebuildingIndexes?.Invoke(this, args);
    }
}
