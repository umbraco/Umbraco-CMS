using Umbraco.Core.Components;

namespace Umbraco.Core.Logging.Viewer
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    // ReSharper disable once UnusedMember.Global
    public class LogViewerComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<ILogViewer>(_ => new JsonLogViewer());
        }
    }
}
