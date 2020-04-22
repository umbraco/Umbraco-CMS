using Umbraco.Core.Composing;
using Umbraco.Core.IO;

namespace Umbraco.Core.Logging.Viewer
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    // ReSharper disable once UnusedMember.Global
    public class LogViewerComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<ILogViewerConfig, LogViewerConfig>();
            composition.SetLogViewer<SerilogJsonLogViewer>();
        }
    }
}
