
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Logging.Viewer
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    // ReSharper disable once UnusedMember.Global
    public class ConfigurationComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<IUmbracoVersion, UmbracoVersion>();
        }

    }
}
