using Examine;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Examine.Search;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Services.QueryBuilders;

internal sealed class ApiContentQuerySelectorBuilder
{
    private readonly DeliveryApiSettings _deliveryApiSettings;
    private readonly string _fallbackGuidValue;

    public ApiContentQuerySelectorBuilder(DeliveryApiSettings deliveryApiSettings)
    {
        _deliveryApiSettings = deliveryApiSettings;

        // A fallback value is needed for Examine queries in case we don't have a value - we can't pass null or empty string
        // It is set to a random guid since this would be highly unlikely to yield any results
        _fallbackGuidValue = Guid.NewGuid().ToString("D");
    }

    public IBooleanOperation Build(SelectorOption selectorOption, IIndex index, string culture, ProtectedAccess protectedAccess, bool preview)
    {
        // Needed for enabling leading wildcards searches
        BaseLuceneSearcher searcher = index.Searcher as BaseLuceneSearcher ?? throw new InvalidOperationException($"Index searcher must be of type {nameof(BaseLuceneSearcher)}.");

        IQuery query = searcher.CreateQuery(
            IndexTypes.Content,
            BooleanOperation.And,
            searcher.LuceneAnalyzer,
            new LuceneSearchOptions { AllowLeadingWildcard = true });

        IBooleanOperation selectorOperation = selectorOption.Values.Length == 1
            ? query.Field(selectorOption.FieldName, selectorOption.Values.First())
            : query.GroupedOr(new[] { selectorOption.FieldName }, selectorOption.Values);

        AddCultureQuery(culture, selectorOperation);

        if (_deliveryApiSettings.MemberAuthorizationIsEnabled())
        {
            AddProtectedAccessQuery(protectedAccess, selectorOperation);
        }

        // when not fetching for preview, make sure the "published" field is "y"
        if (preview is false)
        {
            selectorOperation.And().Field(UmbracoExamineFieldNames.DeliveryApiContentIndex.Published, "y");
        }

        return selectorOperation;
    }

    private void AddCultureQuery(string culture, IBooleanOperation selectorOperation) =>
        selectorOperation
            .And()
            .GroupedOr(
                // Item culture must be either the requested culture or "none"
                new[] { UmbracoExamineFieldNames.DeliveryApiContentIndex.Culture },
                culture.ToLowerInvariant().IfNullOrWhiteSpace(_fallbackGuidValue),
                "none");

    private void AddProtectedAccessQuery(ProtectedAccess protectedAccess, IBooleanOperation selectorOperation)
    {
        var protectedAccessValues = new List<string>();
        if (protectedAccess.MemberKey is not null)
        {
            protectedAccessValues.Add($"u:{protectedAccess.MemberKey}");
        }

        if (protectedAccess.MemberRoles?.Any() is true)
        {
            protectedAccessValues.AddRange(protectedAccess.MemberRoles.Select(r => $"r:{r}"));
        }

        if (protectedAccessValues.Any())
        {
            selectorOperation.And(
                inner => inner
                    .Field(UmbracoExamineFieldNames.DeliveryApiContentIndex.Protected, "n")
                    .Or(protectedAccessInner => protectedAccessInner
                        .GroupedOr(
                            new[] { UmbracoExamineFieldNames.DeliveryApiContentIndex.ProtectedAccess },
                            protectedAccessValues.ToArray())),
                BooleanOperation.Or);
        }
        else
        {
            selectorOperation.And().Field(UmbracoExamineFieldNames.DeliveryApiContentIndex.Protected, "n");
        }
    }
}
