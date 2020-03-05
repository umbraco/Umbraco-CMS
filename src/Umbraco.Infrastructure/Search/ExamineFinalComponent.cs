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
        BackgroundIndexRebuilder _indexRebuilder;
        private readonly IMainDom _mainDom;
        
        public ExamineFinalComponent(BackgroundIndexRebuilder indexRebuilder, IMainDom mainDom)
        {
            _indexRebuilder = indexRebuilder;
            _mainDom = mainDom;
        }

        public void Initialize()
        {
            if (!_mainDom.IsMainDom) return;

            // TODO: Instead of waiting 5000 ms, we could add an event handler on to fulfilling the first request, then start?
            _indexRebuilder.RebuildIndexes(true, 5000);
        }

        public void Terminate()
        {
        }
    }
}
