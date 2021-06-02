using Examine;
using Umbraco.Core.Logging;
using Umbraco.Examine;
using Umbraco.Core.Composing;
using Umbraco.Core;

namespace Umbraco.Web.Search
{

    /// <summary>
    /// Executes after all other examine components have executed
    /// </summary>
    public sealed class ExamineFinalComponent : IComponent
    {   
        private readonly IProfilingLogger _logger;
        private readonly IExamineManager _examineManager;
        BackgroundIndexRebuilder _indexRebuilder;
        private readonly IMainDom _mainDom;
        
        public ExamineFinalComponent(IProfilingLogger logger, IExamineManager examineManager, BackgroundIndexRebuilder indexRebuilder, IMainDom mainDom)
        {
            _logger = logger;
            _examineManager = examineManager;
            _indexRebuilder = indexRebuilder;
            _mainDom = mainDom;
        }

        public void Initialize()
        {
            if (!_mainDom.IsMainDom) return;

            _examineManager.ConfigureIndexes(_mainDom, _logger);

            // TODO: Instead of waiting 5000 ms, we could add an event handler on to fulfilling the first request, then start?
            _indexRebuilder.RebuildIndexes(true, 5000);
        }

        public void Terminate()
        {
        }
    }
}
