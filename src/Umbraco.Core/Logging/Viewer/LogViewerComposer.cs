using Umbraco.Core.Components;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Logging.Viewer
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class LogViewerComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<ILogViewer, JsonLogViewer>();
        }
    }
}
