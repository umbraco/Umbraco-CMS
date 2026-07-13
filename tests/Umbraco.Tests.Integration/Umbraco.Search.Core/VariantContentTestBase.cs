using Umbraco.Cms.Core;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public abstract class VariantContentTestBase : ContentTestBase
{
    [SetUp]
    public async Task SetupTest()
    {
        await GetRequiredService<ILanguageService>().CreateAsync(
            new LanguageBuilder().WithCultureInfo("da-DK").Build(),
            Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.CultureAndSegment)
            .AddPropertyType()
            .WithAlias("title")
            .WithVariations(ContentVariation.Culture)
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("count")
            .WithVariations(ContentVariation.Nothing)
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("message")
            .WithVariations(ContentVariation.CultureAndSegment)
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "Root EN")
            .WithCultureName("da-DK", "Root DA")
            .Build();
        SetTitle(root, "The root title");
        SetMessage(root, "The root message");
        root.SetValue("count", 12);
        ContentService.Save(root);

        Content child = new ContentBuilder()
            .WithKey(ChildKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "Child EN")
            .WithCultureName("da-DK", "Child DA")
            .WithParent(root)
            .Build();
        SetTitle(child, "The child title");
        SetMessage(child, "The child message");
        child.SetValue("count", 34);
        ContentService.Save(child);

        Content grandchild = new ContentBuilder()
            .WithKey(GrandchildKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "Grandchild EN")
            .WithCultureName("da-DK", "Grandchild DA")
            .WithParent(child)
            .Build();
        SetTitle(grandchild, "The grandchild title");
        SetMessage(grandchild, "The grandchild message");
        grandchild.SetValue("count", 56);
        ContentService.Save(grandchild);

        Content greatGrandchild = new ContentBuilder()
            .WithKey(GreatGrandchildKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "Great Grandchild EN")
            .WithCultureName("da-DK", "Great Grandchild DA")
            .WithParent(grandchild)
            .Build();
        SetTitle(greatGrandchild, "The great grandchild title");
        SetMessage(greatGrandchild, "The great grandchild message");
        greatGrandchild.SetValue("count", 78);
        ContentService.Save(greatGrandchild);

        IndexerAndSearcher.Reset();
    }

    private void SetTitle(IContent content, string title)
    {
        content.SetValue("title", $"{title} in English", "en-US");
        content.SetValue("title", $"{title} in Danish", "da-DK");
    }

    private void SetMessage(IContent content, string message)
    {
        content.SetValue("message", $"{message} in English (default)", "en-US");
        content.SetValue("message", $"{message} in English (segment-1)", "en-US", "segment-1");
        content.SetValue("message", $"{message} in English (segment-2)", "en-US", "segment-2");
        content.SetValue("message", $"{message} in Danish (default)", "da-DK");
        content.SetValue("message", $"{message} in Danish (segment-1)", "da-DK", "segment-1");
        content.SetValue("message", $"{message} in Danish (segment-2)", "da-DK", "segment-2");
    }
}
