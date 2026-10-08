using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence;

/// <summary>
///     Verifies that the EF Core element mappings read the element tables as the existing persistence layer writes them.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class ElementDtoMappingTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [Test]
    public async Task Element_Tables_Read_Back_Through_EF_Core()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);
        IContentType elementType = ContentTypeBuilder.CreateSimpleElementType();
        elementType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in elementType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        IElement element = new Element("element", elementType);
        element.SetCultureName("hello", "en-US");
        element.SetCultureName("bonjour", "fr-FR");
        await ElementService.SaveAsync(element, Constants.Security.SuperUserKey, null, CancellationToken.None);
        await ElementService.PublishAsync(element, ["en-US"], Constants.Security.SuperUserKey, CancellationToken.None);

        element = (await ElementService.GetByIdAsync(element.Key, CancellationToken.None))!;
        element.SetCultureName("hello again", "en-US");
        await ElementService.SaveAsync(element, Constants.Security.SuperUserKey, null, CancellationToken.None);

        int englishId = (await LanguageService.GetAsync("en-US"))!.Id;
        int frenchId = (await LanguageService.GetAsync("fr-FR"))!.Id;
        int nodeId = element.Id;

        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        using var scope = NewScopeProvider.CreateScope();
        (ElementDto Element, List<(ElementVersionDto Version, bool Current)> Versions, List<ElementCultureVariationDto> Variations) rows =
            await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(async db =>
            {
                ElementDto elementRow = await db.Elements.SingleAsync(row => row.NodeId == nodeId);

                var versionRows = await db.ElementVersions
                    .Join(db.ContentVersions, version => version.Id, contentVersion => contentVersion.Id, (version, contentVersion) => new { version, contentVersion })
                    .Where(joined => joined.contentVersion.NodeId == nodeId)
                    .Select(joined => new { joined.version, joined.contentVersion.Current })
                    .ToListAsync();

                List<ElementCultureVariationDto> variationRows = await db.ElementCultureVariations
                    .Where(row => row.NodeId == nodeId)
                    .ToListAsync();

                return (elementRow, versionRows.Select(joined => (joined.version, joined.Current)).ToList(), variationRows);
            });
        scope.Complete();

        ElementCultureVariationDto english = rows.Variations.Single(row => row.LanguageId == englishId);
        ElementCultureVariationDto french = rows.Variations.Single(row => row.LanguageId == frenchId);

        Assert.Multiple(() =>
        {
            Assert.That(rows.Element.Published, Is.True);
            Assert.That(rows.Element.Edited, Is.True);

            Assert.That(rows.Versions, Has.Count.EqualTo(2));
            Assert.That(rows.Versions.Single(row => row.Current).Version.Published, Is.False);
            Assert.That(rows.Versions.Single(row => row.Current is false).Version.Published, Is.True);

            Assert.That(rows.Variations, Has.Count.EqualTo(2));
            Assert.That(english.Name, Is.EqualTo("hello again"));
            Assert.That(english.Available, Is.True);
            Assert.That(english.Published, Is.True);
            Assert.That(english.Edited, Is.True);
            Assert.That(french.Name, Is.EqualTo("bonjour"));
            Assert.That(french.Available, Is.True);
            Assert.That(french.Published, Is.False);
            Assert.That(french.Edited, Is.True);
        });
    }
}
