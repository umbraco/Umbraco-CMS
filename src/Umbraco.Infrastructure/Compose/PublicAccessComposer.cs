using Umbraco.Core.Composing;

namespace Umbraco.Web.Compose
{
    /// <summary>
    /// Used to ensure that the public access data file is kept up to date properly
    /// </summary>
    public sealed class PublicAccessComposer : ComponentComposer<PublicAccessComponent>, ICoreComposer
    { }
}
