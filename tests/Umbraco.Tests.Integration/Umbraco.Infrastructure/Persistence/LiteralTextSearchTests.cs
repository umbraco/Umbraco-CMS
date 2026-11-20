// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence;

/// <summary>
///     Text searches find the text they are given, even when it holds characters that a LIKE pattern treats as
///     wildcards or as its escape character.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class LiteralTextSearchTests : UmbracoIntegrationTest
{
    // Each text with wildcard or escape characters is followed by one that a pattern built from it without escaping also matches.
    private static readonly string[] _texts = ["50% off", "500 off", "a_b", "axb", "c[d]", "cd", @"e\", @"e\f"];

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberFilterService MemberFilterService => GetRequiredService<IMemberFilterService>();

    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private IRedirectUrlService RedirectUrlService => GetRequiredService<IRedirectUrlService>();

    private IKeyValueService KeyValueService => GetRequiredService<IKeyValueService>();

    [Test]
    public async Task Media_Queries_Match_Wildcard_Characters_Literally()
    {
        IMediaType mediaType = (await MediaTypeService.GetAsync(Constants.Conventions.MediaTypes.Folder))!;
        foreach (var name in _texts)
        {
            Media media = MediaBuilder.CreateMediaFolder(mediaType, Constants.System.Root);
            media.Name = name;
            MediaService.Save(media);
        }

        IEnumerable<string> Find(Expression<Func<IMedia, bool>> predicate) => MediaService
            .GetPagedChildren(Constants.System.Root, 0, 100, out _, null, ScopeProvider.CreateQuery<IMedia>().Where(predicate), null)
            .Select(x => x.Name!);

        Assert.Multiple(() =>
        {
            Assert.That(Find(x => x.Name!.Contains("0%")), Is.EquivalentTo(new[] { "50% off" }));
            Assert.That(Find(x => x.Name!.Contains("a_b")), Is.EquivalentTo(new[] { "a_b" }));
            Assert.That(Find(x => x.Name!.StartsWith("c[")), Is.EquivalentTo(new[] { "c[d]" }));
            Assert.That(Find(x => x.Name!.EndsWith(@"e\")), Is.EquivalentTo(new[] { @"e\" }));
        });
    }

    [TestCase("0%", "50% off")]
    [TestCase("a_b", "a_b")]
    [TestCase("c[", "c[d]")]
    [TestCase(@"e\", @"e\", @"e\f")]
    public async Task Member_Filters_Match_Wildcard_Characters_Literally(string filter, params string[] expected)
    {
        MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType("searchMember", "Search Member");
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        var index = 0;
        foreach (var name in _texts)
        {
            index++;
            MemberService.Save(MemberBuilder.CreateSimpleMember(memberType, name, $"m{index}@example.com", "password", $"m{index}"));
        }

        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(new MemberFilter { Filter = filter });

        Assert.That(result.Items.Select(x => x.Name), Is.EquivalentTo(expected));
    }

    public static void ConfigureDictionaryValueSearch(IUmbracoBuilder builder)
        => builder.Services.Configure<DictionarySettings>(settings =>
        {
            settings.EnableValueSearch = true;
            settings.KeySearchMode = DictionaryKeySearchMode.Contains;
        });

    [TestCase("50%", "50% off")]
    [TestCase("a_b", "a_b")]
    [TestCase("c[", "c[d]")]
    [TestCase(@"e\", @"e\", @"e\f")]
    public async Task Dictionary_Key_Searches_Match_Wildcard_Characters_Literally(string filter, params string[] expected)
        => Assert.That(await SearchDictionaryKeys(filter), Is.EquivalentTo(expected));

    [TestCase("0%", "50% off")]
    [TestCase("a_b", "a_b")]
    [TestCase("c[", "c[d]")]
    [TestCase(@"e\", @"e\", @"e\f")]
    [ConfigureBuilder(ActionName = nameof(ConfigureDictionaryValueSearch))]
    public async Task Dictionary_Key_And_Value_Searches_Match_Wildcard_Characters_Literally(string filter, params string[] expected)
        => Assert.That(await SearchDictionaryKeys(filter), Is.EquivalentTo(expected));

    [TestCase("0%", "/50%-off")]
    [TestCase("a_b", "/a_b")]
    [TestCase("c[", "/c[d]")]
    [TestCase(@"e\", @"/e\", @"/e\f")]
    public async Task Redirect_Url_Searches_Match_Wildcard_Characters_Literally(string searchTerm, params string[] expected)
    {
        ContentType contentType = await CreateContentType();
        Content content = ContentBuilder.CreateBasicContent(contentType);
        await ContentService.SaveAsync(content, Constants.Security.SuperUserKey, null, CancellationToken.None);
        foreach (var text in _texts)
        {
            RedirectUrlService.Register("/" + text.Replace(' ', '-'), content.Key);
        }

        PagedModel<IRedirectUrl> found = await RedirectUrlService.SearchRedirectUrlsAsync(searchTerm, 0, 100);

        Assert.That(found.Items.Select(x => x.Url), Is.EquivalentTo(expected));
    }

    [Test]
    public async Task Key_Value_Prefix_Lookups_Match_Wildcard_Characters_Literally()
    {
        await KeyValueService.SetValueAsync("literalSearch.a_b.1", "match");
        await KeyValueService.SetValueAsync("literalSearch.axb.1", "wildcard match");

        IReadOnlyDictionary<string, string?>? found = (await KeyValueService.FindByKeyPrefixAsync("literalSearch.a_b.")).Result;

        Assert.That(found?.Keys, Is.EquivalentTo(new[] { "literalSearch.a_b.1" }));
    }

    private async Task<IEnumerable<string>> SearchDictionaryKeys(string filter)
    {
        foreach (var key in _texts)
        {
            await DictionaryItemService.CreateAsync(new DictionaryItem(key), Constants.Security.SuperUserKey);
        }

        IEnumerable<IDictionaryItem> found = await DictionaryItemService.GetDescendantsAsync(null, filter);
        return found.Select(x => x.ItemKey);
    }

    private async Task<ContentType> CreateContentType()
    {
        ContentType contentType = ContentTypeBuilder.CreateBasicContentType("searchPage", "Search Page");
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }
}
