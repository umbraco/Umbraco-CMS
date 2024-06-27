using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class HybridCachingTests : UmbracoIntegrationTestWithContent
{

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddHybridCache();
        services.AddSingleton<IPublishedHybridCache, ContentCache>();
    } 

    private IPublishedHybridCache PublishedHybridCache => GetRequiredService<IPublishedHybridCache>();

    [Test]
    public void Can_Get_Content()
    {
        var textPage = PublishedHybridCache.GetById(Textpage.Key);
        
        Assert.IsNotNull(textPage);
    }
}
