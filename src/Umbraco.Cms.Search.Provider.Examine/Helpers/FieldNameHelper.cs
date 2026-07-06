using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Provider.Examine.Helpers;

public static class FieldNameHelper
{
    public static string FieldName(IndexField field, string fieldValues)
        => FieldName(field.FieldName, fieldValues, field.Segment);

    public static string FieldName(string fieldName, string fieldValues)
        => FieldName(fieldName, fieldValues, null);

    public static string FieldName(string fieldName, string fieldValues, string? segment)
        => $"Field_{fieldName}_{fieldValues}{(segment.IsNullOrWhiteSpace() ? string.Empty : $"_{segment}")}";

    public static string QueryableKeywordFieldName(string fieldName)
        => $"__Query_{fieldName}";

    public static string SegmentedSystemFieldName(string systemFieldName, string? segment)
        => segment.IsNullOrWhiteSpace() ? systemFieldName : $"{systemFieldName}_{segment}";
}
