using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DocumentUrlServiceTest : UmbracoIntegrationTestWithContent
{
    protected IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();

    [Test]
    public async Task RebuildAllUrlsAsync()
    {
        await DocumentUrlService.RebuildAllUrlsAsync();
    }
}
