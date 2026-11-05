using Examine;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine.ContentIndexRebuild;

internal abstract partial class ContentIndexRebuildTestBase
{
    /// <summary>
    ///     Records the content a populator builds value sets from.
    /// </summary>
    private sealed class RecordingValueSetBuilder(IValueSetBuilder<IContent> inner, List<IContent> loadedContent) : IValueSetBuilder<IContent>
    {
        public IEnumerable<ValueSet> GetValueSets(params IContent[] content)
        {
            loadedContent.AddRange(content);
            return inner.GetValueSets(content);
        }
    }

    /// <summary>
    ///     A content index that validates value sets as an Examine index does, and passes the valid ones on.
    /// </summary>
    protected sealed class ValidatingCapturingIndex : BaseIndexProvider, IUmbracoContentIndex
    {
        private readonly Action<IEnumerable<ValueSet>> _capture;

        public ValidatingCapturingIndex(bool publishedValuesOnly, IValueSetValidator validator, Action<IEnumerable<ValueSet>> capture)
            : base(NullLoggerFactory.Instance, "ValidatingCapturingIndex", new TestOptionsMonitor<IndexOptions>(new IndexOptions { Validator = validator }))
        {
            PublishedValuesOnly = publishedValuesOnly;
            _capture = capture;
        }

        public bool EnableDefaultEventHandler => false;

        public bool PublishedValuesOnly { get; }

        public bool SupportProtectedContent => true;

        public override ISearcher Searcher => throw new NotSupportedException();

        public long GetDocumentCount() => throw new NotSupportedException();

        public IEnumerable<string> GetFieldNames() => throw new NotSupportedException();

        public override void CreateIndex()
        {
        }

        public override bool IndexExists() => true;

        protected override void PerformIndexItems(IEnumerable<ValueSet> values, Action<IndexOperationEventArgs> onComplete)
            => _capture(values);

        protected override void PerformDeleteFromIndex(IEnumerable<string> itemIds, Action<IndexOperationEventArgs> onComplete)
            => throw new NotSupportedException();
    }
}
