using Examine;
using Examine.Search;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Provider.Examine.Helpers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Constants = Umbraco.Cms.Search.Provider.Examine.Constants;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;

public class InvariantSortableIndexTests : IndexTestBase
{
    private IContentType ContentType { get; set; } = null!;

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanGetSortedTitles(bool publish)
    {
        await CreateTitleDocuments(["C Title", "A Title", "B Title"]);

        IIndex index = GetIndex(publish
            ? Cms.Search.Core.Constants.IndexAliases.PublishedContent
            : Cms.Search.Core.Constants.IndexAliases.DraftContent);

        var fieldName = FieldNameHelper.FieldName("sortableTitle", Constants.FieldValues.Texts);
        ISearchResults results = index.Searcher.CreateQuery().All().OrderBy(new SortableField(fieldName, SortType.String)).Execute();
        var values = results.SelectMany(x => x.Values.Where(value => value.Key == fieldName)).Select(x => x.Value).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(results, Is.Not.Empty);
            Assert.That(values.First(), Is.EqualTo("A Title"));
            Assert.That(values.Skip(1).First(), Is.EqualTo("B Title"));
            Assert.That(values.Last(), Is.EqualTo("C Title"));
        });
    }

    private async Task CreateTitleDocType()
    {
        ContentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("sortableTitle")
            .WithDataTypeId(Cms.Core.Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Cms.Core.Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(ContentType, Cms.Core.Constants.Security.SuperUserKey);
    }

    private async Task CreateTitleDocuments(string[] values)
    {
        await CreateTitleDocType();

        await WaitForIndexing(Cms.Search.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            foreach (var stringValue in values)
            {
                Content document = new ContentBuilder()
                    .WithContentType(ContentType)
                    .WithName($"document-{stringValue}")
                    .WithPropertyValues(new { sortableTitle = stringValue, title = stringValue })
                    .Build();

                SaveAndPublish(document);
            }

            return Task.CompletedTask;
        });
    }
}
