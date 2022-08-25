using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Imaging.ImageSharp
{
    public class ImageSharpComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
            => builder.AddUmbracoImageSharp();
    }
}
