using Umbraco.Core.Compose;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Logging.Viewer
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    // ReSharper disable once UnusedMember.Global
    public class LogViewerComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.SetLogViewer(_ => new JsonLogViewer(composition.Logger));
        }
    }
}
