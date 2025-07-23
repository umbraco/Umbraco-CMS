using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.Examine;

public sealed class AddExamineComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder
            .AddExamine()
            .AddExamineIndexes();
}
