using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class HybridCachingTests : UmbracoIntegrationTestWithContent
{

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddHybridCache();
        services.AddSingleton<IPublishedHybridCache, ContentCache>();
        services.AddSingleton<INuCacheContentRepository, NuCacheContentRepository>();
        services.AddSingleton<IContentCacheDataSerializerFactory>(s =>
        {
            IOptions<NuCacheSettings> options = s.GetRequiredService<IOptions<NuCacheSettings>>();
            switch (options.Value.NuCacheSerializerType)
            {
                case NuCacheSerializerType.JSON:
                    return new JsonContentNestedDataSerializerFactory();
                case NuCacheSerializerType.MessagePack:
                    return ActivatorUtilities.CreateInstance<MsgPackContentNestedDataSerializerFactory>(s);
                default:
                    throw new IndexOutOfRangeException();
            }
        });
    }

    private IPublishedHybridCache PublishedHybridCache => GetRequiredService<IPublishedHybridCache>();

    [Test]
    public void Can_Get_Content_By_Id()
    {
        var textPage = PublishedHybridCache.GetById(Textpage.Id);

        Assert.IsNotNull(textPage);
    }
}
