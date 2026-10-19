using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public abstract class InvariantContentTestBase : ContentTestBase
{
    [SetUp]
    public virtual async Task SetupTest()
    {
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("count")
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("tags")
            .WithDataTypeId(Constants.DataTypes.Tags)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Tags)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithName("Root")
            .WithPropertyValues(
                new
                {
                    title = "The root title",
                    count = 12,
                    tags = "[\"tag1\",\"tag2\"]"
                })
            .Build();
        ContentService.Save(root);

        Content child = new ContentBuilder()
            .WithKey(ChildKey)
            .WithContentType(contentType)
            .WithName("Child")
            .WithParent(root)
            .WithPropertyValues(
                new
                {
                    title = "The child title",
                    count = 34,
                    tags = "[\"tag3\",\"tag4\"]"
                })
            .Build();
        ContentService.Save(child);

        Content grandchild = new ContentBuilder()
            .WithKey(GrandchildKey)
            .WithContentType(contentType)
            .WithName("Grandchild")
            .WithParent(child)
            .WithPropertyValues(
                new
                {
                    title = "The grandchild title",
                    count = 56,
                    tags = "[\"tag5\",\"tag6\"]"
                })
            .Build();
        ContentService.Save(grandchild);

        Content greatGrandchild = new ContentBuilder()
            .WithKey(GreatGrandchildKey)
            .WithContentType(contentType)
            .WithName("Great Grandchild")
            .WithParent(grandchild)
            .WithPropertyValues(
                new
                {
                    title = "The great grandchild title",
                    count = 78,
                    tags = "[\"tag7\",\"tag8\"]"
                })
            .Build();
        ContentService.Save(greatGrandchild);

        IndexerAndSearcher.Reset();
    }
}
