using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.Request;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public abstract class ApiContentPathResolverTestBase : ApiContentRequestTestBase
{
    protected IApiContentPathResolver ApiContentPathResolver => GetRequiredService<IApiContentPathResolver>();

    protected IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();
}
