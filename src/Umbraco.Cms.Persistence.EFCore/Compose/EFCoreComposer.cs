using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Compose;

public class EFCoreComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => builder.Services.AddUmbracoEFCore(builder.Config);
}
