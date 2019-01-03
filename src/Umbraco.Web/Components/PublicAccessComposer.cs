using Umbraco.Core;
using Umbraco.Core.Components;

namespace Umbraco.Web.Components
{
    /// <summary>
    /// Used to ensure that the public access data file is kept up to date properly
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class PublicAccessComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<PublicAccessComponent>();
        }
    }
}