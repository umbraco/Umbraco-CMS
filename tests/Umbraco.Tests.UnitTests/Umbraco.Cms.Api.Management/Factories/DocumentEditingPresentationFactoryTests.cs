using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Mapping.Content;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class DocumentEditingPresentationFactoryTests
{
    [Test]
    public async Task CreateUpdateRequestModelAsync_Orders_Values_And_Variants_Like_The_Response_Models()
    {
        // the update request model is the base for PATCH operations, so index-based patch paths must address
        // values and variants in the same order as they are returned by the Management API
        IContent content = ContentMapDefinitionTests.CreateContent(
            ["en-US", "da-DK"],
            ContentMapDefinitionTests.CreateProperty("title", ("en-US", null), ("da-DK", "segment-b")),
            ContentMapDefinitionTests.CreateProperty("body", ("en-US", "segment-a"), ("da-DK", null), (null, null)));

        var factory = new DocumentEditingPresentationFactory(
            ContentMapDefinitionTests.CreatePropertyEditorCollection(),
            Mock.Of<IDataValueEditorFactory>(),
            Mock.Of<ITemplateService>());

        UpdateDocumentRequestModel model = await factory.CreateUpdateRequestModelAsync(content);

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(
                new (string?, string?, string)[]
                {
                    (null, null, "body"),
                    ("da-DK", null, "body"),
                    ("da-DK", "segment-b", "title"),
                    ("en-US", null, "title"),
                    ("en-US", "segment-a", "body"),
                },
                model.Values.Select(value => (value.Culture, value.Segment, value.Alias)).ToArray());
            CollectionAssert.AreEqual(
                new (string?, string?)[]
                {
                    ("da-DK", null),
                    ("da-DK", "segment-a"),
                    ("da-DK", "segment-b"),
                    ("en-US", null),
                    ("en-US", "segment-a"),
                    ("en-US", "segment-b"),
                },
                model.Variants.Select(variant => (variant.Culture, variant.Segment)).ToArray());
        });
    }
}
