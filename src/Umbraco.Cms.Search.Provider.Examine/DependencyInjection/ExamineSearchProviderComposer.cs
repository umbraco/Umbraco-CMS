using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.DependencyInjection;

namespace Umbraco.Cms.Search.Provider.Examine.DependencyInjection;

/// <summary>
/// Registers the search core and the Examine search provider.
/// </summary>
public sealed class ExamineSearchProviderComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
        => builder
            .AddSearchCore()
            .AddExamineSearchProvider();
}
