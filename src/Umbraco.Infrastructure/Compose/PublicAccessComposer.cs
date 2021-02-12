using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Compose
{
    /// <summary>
    /// Used to ensure that the public access data file is kept up to date properly
    /// </summary>
    public sealed class PublicAccessComposer : ComponentComposer<PublicAccessComponent>, ICoreComposer
    { }
}
