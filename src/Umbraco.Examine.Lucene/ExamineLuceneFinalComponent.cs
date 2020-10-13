using Microsoft.Extensions.Logging;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Examine
{
    public class ExamineLuceneFinalComponent : IComponent
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IExamineManager _examineManager;
        private readonly IMainDom _mainDom;

        public ExamineLuceneFinalComponent(ILoggerFactory loggerFactory, IExamineManager examineManager, IMainDom mainDom)
        {
            _loggerFactory = loggerFactory;
            _examineManager = examineManager;
            _mainDom = mainDom;
        }

        public void Initialize()
        {
            if (!_mainDom.IsMainDom) return;

            // Ensures all lucene based indexes are unlocked and ready to go
            _examineManager.ConfigureIndexes(_mainDom, _loggerFactory.CreateLogger<IExamineManager>());
        }

        public void Terminate()
        {
        }
    }
}
