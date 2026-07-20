using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.DependencyInjection;

namespace Umbraco.Cms.Search.Provider.Examine.DependencyInjection;

public sealed class ExamineSearchProviderComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder
            .AddSearchCore()
            .AddExamineSearchProvider();
}
