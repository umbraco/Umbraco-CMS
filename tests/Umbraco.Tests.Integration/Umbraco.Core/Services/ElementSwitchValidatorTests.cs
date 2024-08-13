using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ElementSwitchValidatorTests : UmbracoIntegrationTest
{
    private IElementSwitchValidator ElementSwitchValidator => GetRequiredService<IElementSwitchValidator>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    [TestCase(new[] { true }, 0, true, true, TestName = "E=>E No Ancestor or children")]
    [TestCase(new[] { false }, 0, false, true, TestName = "D=>D No Ancestor or children")]
    [TestCase(new[] { true }, 0, false, true, TestName = "E=>D No Ancestor or children")]
    [TestCase(new[] { false }, 0, true, true, TestName = "D=>E No Ancestor or children")]

    [TestCase(new[] { true, true }, 1, true, true, TestName = "E Valid Parent")]
    [TestCase(new[] { true, true }, 0, true, true, TestName = "E Valid Child")]
    [TestCase(new[] { false, false }, 1, false, true, TestName = "D Valid Parent")]
    [TestCase(new[] { false, false }, 0, false, true, TestName = "D Valid Child")]
    [TestCase(new[] { false, false }, 1, true, false, TestName = "E InValid Parent")]
    [TestCase(new[] { false, false }, 0, true, true, TestName = "E InValid Child")]
    [TestCase(new[] { true, true }, 1, false, false, TestName = "D InValid Parent")]
    [TestCase(new[] { true, true }, 0, false, true, TestName = "D InValid Child")]

    [TestCase(new[] { true, false, false, true, false }, 2, true, false, TestName = "D=>E InValid Child, Invalid Parent")]
    [TestCase(new[] { false, true, false, true, false }, 2, true, false, TestName = "D=>E InValid Child, Invalid Ancestor")]
    [TestCase(new[] { true, false, false, true, true }, 2, true, false, TestName = "D=>E Valid Children, Invalid Parent")]
    [TestCase(new[] { false, true, false, true, true }, 2, true, false, TestName = "D=>E Valid Children, Invalid Ancestor")]

    [TestCase(new[] { false, false, false, false, false }, 2, true, false, TestName = "D=>E mismatch")]
    [TestCase(new[] { false, false, true, false, false }, 2, false, true, TestName = "D=>E correction")]

    [TestCase(new[] { true, true, true, true, true }, 2, false, false, TestName = "E=>D mismatch")]
    [TestCase(new[] { true, true, false, true, true }, 2, true, true, TestName = "E=>D correction")]

    [LongRunning]
    public async Task AncestorsAreAligned(
        bool[] isElementDoctypeChain,
        int itemToTestIndex,
        bool itemToTestNewIsElementValue,
        bool validationShouldPass)
    {
        // Arrange
        IContentType? parentItem = null;
        IContentType? itemToTest = null;
        for (var index = 0; index < isElementDoctypeChain.Length; index++)
        {
            var itemIsElement = isElementDoctypeChain[index];
            var builder = new ContentTypeBuilder()
                .WithIsElement(itemIsElement);
            if (parentItem is not null)
            {
                builder.WithParentContentType(parentItem);
            }

            var contentType = builder.Build();
            await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
            parentItem = contentType;
            if (index == itemToTestIndex)
            {
                itemToTest = contentType;
            }
        }

        // Act
        itemToTest!.IsElement = itemToTestNewIsElementValue;
        var result = await ElementSwitchValidator.AncestorsAreAlignedAsync(itemToTest);

        // Assert
        Assert.AreEqual(result, validationShouldPass);
    }

    [TestCase(new[] { true }, 0, true, true, TestName = "E=>E No Ancestor or children")]
    [TestCase(new[] { false }, 0, false, true, TestName = "D=>D No Ancestor or children")]
    [TestCase(new[] { true }, 0, false, true, TestName = "E=>D No Ancestor or children")]
    [TestCase(new[] { false }, 0, true, true, TestName = "D=>E No Ancestor or children")]

    [TestCase(new[] { true, true }, 1, true, true, TestName = "E Valid Parent")]
    [TestCase(new[] { true, true }, 0, true, true, TestName = "E Valid Child")]
    [TestCase(new[] { false, false }, 1, false, true, TestName = "D Valid Parent")]
    [TestCase(new[] { false, false }, 0, false, true, TestName = "D Valid Child")]
    [TestCase(new[] { false, false }, 1, true, true, TestName = "E InValid Parent")]
    [TestCase(new[] { false, false }, 0, true, false, TestName = "E InValid Child")]
    [TestCase(new[] { true, true }, 1, false, true, TestName = "D InValid Parent")]
    [TestCase(new[] { true, true }, 0, false, false, TestName = "D InValid Child")]

    [TestCase(new[] { true, false, false, true, false }, 2, true, false, TestName = "D=>E InValid Child, Invalid Parent")]
    [TestCase(new[] { false, true, false, true, false }, 2, true, false, TestName = "D=>E InValid Child, Invalid Ancestor")]
    [TestCase(new[] { true, false, false, true, true }, 2, true, true, TestName = "D=>E Valid Children, Invalid Parent")]
    [TestCase(new[] { false, true, false, true, true }, 2, true, true, TestName = "D=>E Valid Children, Invalid Ancestor")]

    [TestCase(new[] { false, false, false, false, false }, 2, true, false, TestName = "D=>E mismatch")]
    [TestCase(new[] { false, false, true, false, false }, 2, false, true, TestName = "D=>E correction")]

    [TestCase(new[] { true, true, true, true, true }, 2, false, false, TestName = "E=>D mismatch")]
    [TestCase(new[] { true, true, false, true, true }, 2, true, true, TestName = "E=>D correction")]

    [LongRunning]
    public async Task DescendantsAreAligned(
        bool[] isElementDoctypeChain,
        int itemToTestIndex,
        bool itemToTestNewIsElementValue,
        bool validationShouldPass)
    {
        // Arrange
        IContentType? parentItem = null;
        IContentType? itemToTest = null;
        for (var index = 0; index < isElementDoctypeChain.Length; index++)
        {
            var itemIsElement = isElementDoctypeChain[index];
            var builder = new ContentTypeBuilder()
                .WithIsElement(itemIsElement);
            if (parentItem is not null)
            {
                builder.WithParentContentType(parentItem);
            }

            var contentType = builder.Build();
            await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
            parentItem = contentType;
            if (index == itemToTestIndex)
            {
                itemToTest = contentType;
            }
        }

        // Act
        itemToTest!.IsElement = itemToTestNewIsElementValue;
        var result = await ElementSwitchValidator.DescendantsAreAlignedAsync(itemToTest);

        // Assert
        Assert.AreEqual(result, validationShouldPass);
    }
}
