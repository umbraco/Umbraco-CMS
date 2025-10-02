using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.DevelopmentMode.Backoffice.DependencyInjection;

public class BackofficeDevelopmentComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => builder.AddBackofficeDevelopment();
}
