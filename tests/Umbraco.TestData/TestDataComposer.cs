using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.TestData.Extensions;

namespace Umbraco.TestData;

public class TestDataComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => builder.AddUmbracoTestData();
}
