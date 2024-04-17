using System.Text.RegularExpressions;
using Umbraco.Cms.Api.Delivery.Indexing.Sorts;

namespace Umbraco.Cms.Api.Delivery.Querying.Filters;

public sealed partial class CreateDateFilter : ContainsFilterBase
{
    protected override string FieldName => CreateDateSortIndexer.FieldName;

    protected override Regex QueryParserRegex => CreateDateRegex();

    [GeneratedRegex("createDate(?<operator>[><:]{1,2})(?<value>.*)", RegexOptions.IgnoreCase)]
    public static partial Regex CreateDateRegex();
}
