using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.TestData.Extensions;

// see https://github.com/Shazwazza/UmbracoScripts/tree/master/src/LoadTesting

namespace Umbraco.TestData;

public class LoadTestComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => builder.AddUmbracoTestData();
}
