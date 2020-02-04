using Examine;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Umbraco.Examine
{
    public class ExamineLuceneFinalComponent : IComponent
    {
        private readonly IProfilingLogger _logger;
        private readonly IExamineManager _examineManager;
        private readonly IMainDom _mainDom;

        public ExamineLuceneFinalComponent(IProfilingLogger logger, IExamineManager examineManager, IMainDom mainDom)
        {
            _logger = logger;
            _examineManager = examineManager;
            _mainDom = mainDom;
        }

        public void Initialize()
        {
            if (!_mainDom.IsMainDom) return;

            // Ensures all lucene based indexes are unlocked and ready to go
            _examineManager.ConfigureIndexes(_mainDom, _logger);
        }

        public void Terminate()
        {
        }
    }
}
