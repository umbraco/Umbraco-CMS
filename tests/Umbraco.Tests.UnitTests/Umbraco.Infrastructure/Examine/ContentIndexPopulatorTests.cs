using Examine;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Examine;

[TestFixture]
public class ContentIndexPopulatorTests
{
    [Test]
    public void GetPropertyAliasesForIncludedFields_Returns_Null_When_Index_Has_No_Validator()
        => Assert.That(ContentIndexPopulator.GetPropertyAliasesForIncludedFields([new TestIndex(null)]), Is.Null);

    [Test]
    public void GetPropertyAliasesForIncludedFields_Returns_Null_When_Validator_Does_Not_Restrict_Fields()
        => Assert.That(ContentIndexPopulator.GetPropertyAliasesForIncludedFields([new TestIndex(new ValueSetValidator(null, null, null, ["excluded"]))]), Is.Null);

    [Test]
    public void GetPropertyAliasesForIncludedFields_Returns_Null_When_Validator_Is_Not_A_ValueSetValidator()
        => Assert.That(ContentIndexPopulator.GetPropertyAliasesForIncludedFields([new TestIndex(Mock.Of<IValueSetValidator>())]), Is.Null);

    [Test]
    public void GetPropertyAliasesForIncludedFields_Returns_Null_When_Index_Is_Not_An_Examine_Index()
        => Assert.That(ContentIndexPopulator.GetPropertyAliasesForIncludedFields([Mock.Of<IIndex>()]), Is.Null);

    [Test]
    public void GetPropertyAliasesForIncludedFields_Returns_Null_When_Any_Index_Needs_All_Fields()
    {
        string[]? propertyAliases = ContentIndexPopulator.GetPropertyAliasesForIncludedFields(
        [
            new TestIndex(new ValueSetValidator(null, null, ["title"], null)),
            new TestIndex(null),
        ]);

        Assert.That(propertyAliases, Is.Null);
    }

    [Test]
    public void GetPropertyAliasesForIncludedFields_Returns_Aliases_That_May_Produce_Included_Fields()
    {
        string[]? propertyAliases = ContentIndexPopulator.GetPropertyAliasesForIncludedFields(
        [
            new TestIndex(new ValueSetValidator(null, null, ["title", "summary_en-us", "__Raw_body_da-dk", "blocks.items[0].text_en-us", "meta_title"], null)),
            new TestIndex(new ValueSetValidator(null, null, ["Keywords"], null)),
        ]);

        Assert.That(propertyAliases, Is.Not.Null);
        Assert.That(propertyAliases, Is.SupersetOf(new[] { "title", "summary", "body", "blocks", "meta_title", "Keywords", Constants.Conventions.Content.UrlName }));
    }

    [Test]
    public void GetPropertyAliasesForIncludedFields_Returns_Null_When_Too_Many_Fields_Are_Included()
    {
        var includeFields = Enumerable.Range(0, 1001).Select(i => $"field{i}").ToArray();

        Assert.That(ContentIndexPopulator.GetPropertyAliasesForIncludedFields([new TestIndex(new ValueSetValidator(null, null, includeFields, null))]), Is.Null);
    }

    private sealed class TestIndex(IValueSetValidator? validator)
        : BaseIndexProvider(NullLoggerFactory.Instance, "TestIndex", new TestOptionsMonitor<IndexOptions>(new IndexOptions { Validator = validator }))
    {
        public override ISearcher Searcher => throw new NotSupportedException();

        public override void CreateIndex()
        {
        }

        public override bool IndexExists() => true;

        protected override void PerformIndexItems(IEnumerable<ValueSet> values, Action<IndexOperationEventArgs> onComplete)
            => throw new NotSupportedException();

        protected override void PerformDeleteFromIndex(IEnumerable<string> itemIds, Action<IndexOperationEventArgs> onComplete)
            => throw new NotSupportedException();
    }
}
