using System.Text.RegularExpressions;
using Umbraco.Cms.Api.Delivery.Indexing.Sorts;

namespace Umbraco.Cms.Api.Delivery.Querying.Filters;

public sealed partial class UpdateDateFilter : ContainsFilterBase
{
    protected override string FieldName => UpdateDateSortIndexer.FieldName;

    protected override Regex QueryParserRegex => UpdateDateRegex();

    [GeneratedRegex("updateDate(?<operator>[><:]{1,2})(?<value>.*)", RegexOptions.IgnoreCase)]
    public static partial Regex UpdateDateRegex();
}
