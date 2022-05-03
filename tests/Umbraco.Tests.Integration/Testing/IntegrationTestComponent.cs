// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading;
using System.Threading.Tasks;
using Examine;
using Examine.Lucene.Providers;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    /// <summary>
    /// A component to customize some services to work nicely with integration tests
    /// </summary>
    public class IntegrationTestComponent : IComponent
    {
        private readonly IExamineManager _examineManager;

        public IntegrationTestComponent(IExamineManager examineManager) => _examineManager = examineManager;

        public async Task InitializeAsync(CancellationToken cancellationToken) => ConfigureExamineIndexes();

        public async Task TerminateAsync(CancellationToken cancellationToken)
        {
        }

        /// <summary>
        /// Configure all indexes to run sync (non-backbround threads) and to use RAMDirectory
        /// </summary>
        private void ConfigureExamineIndexes()
        {
            foreach (IIndex index in _examineManager.Indexes)
            {
                if (index is LuceneIndex luceneIndex)
                {
                    luceneIndex.WithThreadingMode(IndexThreadingMode.Synchronous);
                }
            }
        }
    }
}
