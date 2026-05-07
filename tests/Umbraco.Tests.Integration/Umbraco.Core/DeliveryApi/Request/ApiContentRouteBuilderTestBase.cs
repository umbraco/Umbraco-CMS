using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.Request;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public abstract class ApiContentRouteBuilderTestBase : ApiContentRequestTestBase
{
    protected IPublishedContent GetPublishedContent(Guid key)
    {
        var publishedContent = ClearAndEnsureUmbracoContext().Content.GetById(key);
        Assert.IsNotNull(publishedContent);

        return publishedContent;
    }

    protected IUmbracoContext ClearAndEnsureUmbracoContext()
    {
        UmbracoContextAccessor.Clear();
        return UmbracoContextFactory.EnsureUmbracoContext().UmbracoContext;
    }
}
