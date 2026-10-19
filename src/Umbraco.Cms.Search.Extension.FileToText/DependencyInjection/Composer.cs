using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Search.Extension.FileToText.DependencyInjection;

internal sealed class Composer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.Services.AddFileToText();
}
