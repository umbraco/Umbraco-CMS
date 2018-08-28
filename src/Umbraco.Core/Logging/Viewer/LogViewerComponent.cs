using Umbraco.Core.Components;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Logging.Viewer
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class LogViewerComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(Composition composition)
        {
            composition.Container.RegisterSingleton<ILogViewer, JsonLogViewer>();
        }
    }
}
