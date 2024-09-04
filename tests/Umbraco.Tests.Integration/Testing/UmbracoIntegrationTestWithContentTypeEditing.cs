using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Testing;

public class UmbracoIntegrationTestWithContentTypeEditing : UmbracoIntegrationTest
{
    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected ContentType ContentType { get; private set; }

    [SetUp]
    public new void Setup() => CreateTestData();

    protected async void CreateTestData()
    {
        ContentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage");
        ContentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
    }
}
