using Examine;
using Examine.LuceneEngine.Providers;
using Umbraco.Core.Composing;
using Umbraco.Examine;

namespace Umbraco.Tests.Integration.Testing
{
    /// <summary>
    /// A component to customize some services to work nicely with integration tests
    /// </summary>
    public class IntegrationTestComponent : IComponent
    {
        private readonly IExamineManager _examineManager;

        public IntegrationTestComponent(IExamineManager examineManager)
        {
            _examineManager = examineManager;
        }

        public void Initialize()
        {
            ConfigureExamineIndexes();
        }

        public void Terminate()
        {
        }

        /// <summary>
        /// Configure all indexes to run sync (non-backbround threads) and to use RAMDirectory
        /// </summary>
        private void ConfigureExamineIndexes()
        {
            foreach (var index in _examineManager.Indexes)
            {
                if (index is LuceneIndex luceneIndex)
                {
                    luceneIndex.ProcessNonAsync();
                }
            }
        }
    }
}
