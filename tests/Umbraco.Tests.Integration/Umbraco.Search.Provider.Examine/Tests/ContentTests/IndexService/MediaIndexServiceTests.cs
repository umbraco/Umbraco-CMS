using Examine;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;

public class MediaIndexServiceTests : IndexTestBase
{
    [Test]
    public async Task CanIndexAnyMedia()
    {
        await CreateMediaAsync();

        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.DraftMedia);

        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results.TotalItemCount, Is.EqualTo(1));
    }

    private async Task CreateMediaAsync()
    {
        IMediaType mediaType = new MediaTypeBuilder()
            .WithAlias("theMediaType")
            .AddPropertyGroup()
            .AddPropertyType()
            .WithAlias("altText")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Done()
            .Build();
        await GetRequiredService<IMediaTypeService>().CreateAsync(mediaType, Constants.Security.SuperUserKey);

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.DraftMedia, () =>
        {
            GetRequiredService<IMediaService>().Save(
                new MediaBuilder()
                    .WithMediaType(mediaType)
                    .WithName("The Media")
                    .WithPropertyValues(new { altText = "The media alt text" })
                    .Build());
            return Task.CompletedTask;
        });
    }
}
