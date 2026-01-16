using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public class ElementServiceTests : UmbracoIntegrationTest
{
    private IElementService ElementService => GetRequiredService<IElementService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    [Test]
    public async Task Can_Save_Element()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // Arrange
        // TODO ELEMENTS: This seems to be a leftover from early POC implementation; IElementService.Create() is not
        //                used anywhere else than this. Should probably be removed.
        var element = ElementService.Create("My Element", elementType.Alias);
        element.SetValue("title", "The Element Title");

        // Act
        var result = ElementService.Save(element);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(OperationResultType.Success));

        element = ElementService.GetById(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.HasIdentity, Is.True);
        Assert.That(element.Name, Is.EqualTo("My Element"));
        Assert.That(element.GetValue<string>("title"), Is.EqualTo("The Element Title"));
    }
}
