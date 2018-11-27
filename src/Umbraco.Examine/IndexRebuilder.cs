using System.Linq;
using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Utility to rebuild all indexes ensuring minimal data queries
    /// </summary>
    public class IndexRebuilder
    {
        public IExamineManager ExamineManager { get; }
        private readonly ContentIndexPopulator _contentIndexPopulator;
        private readonly MediaIndexPopulator _mediaIndexPopulator;

        public IndexRebuilder(IExamineManager examineManager, ContentIndexPopulator contentIndexPopulator, MediaIndexPopulator mediaIndexPopulator)
        {
            ExamineManager = examineManager;
            _contentIndexPopulator = contentIndexPopulator;
            _mediaIndexPopulator = mediaIndexPopulator;
        }

        public void RebuildIndexes(bool onlyEmptyIndexes)
        {
            var indexes = (onlyEmptyIndexes
                ? ExamineManager.IndexProviders.Values.Where(x => x.IndexExists())
                : ExamineManager.IndexProviders.Values).ToList();

            var contentIndexes = indexes.Where(x => x is IUmbracoContentIndexer).ToArray();
            var mediaIndexes = indexes.Where(x => x is IUmbracoContentIndexer).ToArray();
            var nonUmbracoIndexes = indexes.Except(contentIndexes).Except(mediaIndexes).ToArray();

            foreach(var index in indexes)
                index.CreateIndex(); // clear the index

            //reindex all content/media indexes with the same data source/lookup
            _contentIndexPopulator.Populate(contentIndexes);
            _mediaIndexPopulator.Populate(mediaIndexes);

            //then do the rest
            foreach (var index in nonUmbracoIndexes)
            {
                index.CreateIndex();
                //TODO: How to rebuild?
            }
                
        }
    }
}
