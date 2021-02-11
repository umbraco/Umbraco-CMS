// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Extensions;

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
