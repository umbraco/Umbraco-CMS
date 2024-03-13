using Examine.Search;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Services.QueryBuilders;

internal sealed class ApiContentQueryFilterBuilder
{
    private readonly IDictionary<string, FieldType> _fieldTypes;
    private readonly ILogger _logger;
    private readonly string _fallbackGuidValue;

    public ApiContentQueryFilterBuilder(IDictionary<string, FieldType> fieldTypes, ILogger logger)
    {
        _fieldTypes = fieldTypes;
        _logger = logger;

        // A fallback value is needed for Examine queries in case we don't have a value - we can't pass null or empty string
        // It is set to a random guid since this would be highly unlikely to yield any results
        _fallbackGuidValue = Guid.NewGuid().ToString("D");
    }

    public void Append(IList<FilterOption> filterOptions, IBooleanOperation queryOperation)
    {
        foreach (FilterOption filterOption in filterOptions)
        {
            if (_fieldTypes.TryGetValue(filterOption.FieldName, out FieldType fieldType) is false)
            {
                _logger.LogWarning(
                    "Filter implementation for field name {FieldName} does not match an index handler implementation, cannot resolve field type.",
                    filterOption.FieldName);
                continue;
            }

            var values = filterOption.Values.Any()
                ? filterOption.Values
                : new[] { _fallbackGuidValue };

            switch (filterOption.Operator)
            {
                case FilterOperation.Is:
                    ApplyExactFilter(queryOperation.And(), filterOption.FieldName, values, fieldType);
                    break;
                case FilterOperation.IsNot:
                    ApplyExactFilter(queryOperation.Not(), filterOption.FieldName, values, fieldType);
                    break;
                case FilterOperation.Contains:
                    ApplyContainsFilter(queryOperation.And(), filterOption.FieldName, values);
                    break;
                case FilterOperation.DoesNotContain:
                    ApplyContainsFilter(queryOperation.Not(), filterOption.FieldName, values);
                    break;
                case FilterOperation.LessThan:
                case FilterOperation.LessThanOrEqual:
                case FilterOperation.GreaterThan:
                case FilterOperation.GreaterThanOrEqual:
                    ApplyRangeFilter(queryOperation.And(), filterOption.FieldName, values, fieldType, filterOption.Operator);
                    break;
                default:
                    continue;
            }
        }
    }

    private void ApplyExactFilter(IQuery query, string fieldName, string[] values, FieldType fieldType)
    {
        switch (fieldType)
        {
            case FieldType.Number:
                ApplyExactNumberFilter(query, fieldName, values);
                break;
            case FieldType.Date:
                ApplyExactDateFilter(query, fieldName, values);
                break;
            default:
                ApplyExactStringFilter(query, fieldName, values);
                break;
        }
    }

    private void ApplyExactNumberFilter(IQuery query, string fieldName, string[] values)
    {
        if (values.Length == 1)
        {
            if (TryParseIntFilterValue(values.First(), out int intValue))
            {
                query.Field(fieldName, intValue);
            }
        }
        else
        {
            int[] intValues = values
                .Select(value => TryParseIntFilterValue(value, out int intValue) ? intValue : (int?)null)
                .Where(intValue => intValue.HasValue)
                .Select(intValue => intValue!.Value)
                .ToArray();

            AddGroupedOrFilter(query, fieldName, intValues);
        }
    }

    private void ApplyExactDateFilter(IQuery query, string fieldName, string[] values)
    {
        if (values.Length == 1)
        {
            if (TryParseDateTimeFilterValue(values.First(), out DateTime dateValue))
            {
                query.Field(fieldName, dateValue);
            }
        }
        else
        {
            DateTime[] dateValues = values
                .Select(value => TryParseDateTimeFilterValue(value, out DateTime dateValue) ? dateValue : (DateTime?)null)
                .Where(dateValue => dateValue.HasValue)
                .Select(dateValue => dateValue!.Value)
                .ToArray();

            AddGroupedOrFilter(query, fieldName, dateValues);
        }
    }

    private void ApplyExactStringFilter(IQuery query, string fieldName, string[] values)
    {
        if (values.Length == 1)
        {
            query.Field(fieldName, values.First());
        }
        else
        {
            AddGroupedOrFilter(query, fieldName, values);
        }
    }

    private void ApplyContainsFilter(IQuery query, string fieldName, string[] values)
    {
        if (values.Length == 1)
        {
            // The trailing wildcard is added automatically
            query.Field(fieldName, (IExamineValue)new ExamineValue(Examineness.ComplexWildcard, $"*{values[0]}"));
        }
        else
        {
            // The trailing wildcard is added automatically
            IExamineValue[] examineValues = values
                .Select(value => (IExamineValue)new ExamineValue(Examineness.ComplexWildcard, $"*{value}"))
                .ToArray();
            query.GroupedOr(new[] { fieldName }, examineValues);
        }
    }

    private void ApplyRangeFilter(IQuery query, string fieldName, string[] values, FieldType fieldType, FilterOperation filterOperation)
    {
        switch (fieldType)
        {
            case FieldType.Number:
                ApplyRangeNumberFilter(query, fieldName, values, filterOperation);
                break;
            case FieldType.Date:
                ApplyRangeDateFilter(query, fieldName, values, filterOperation);
                break;
            default:
                _logger.LogWarning("Range filtering cannot be used with String fields. Only Number and Date fields support range filtering.");
                break;
        }
    }

    private void ApplyRangeNumberFilter(IQuery query, string fieldName, string[] values, FilterOperation filterOperation)
    {
        if (TryParseIntFilterValue(values.First(), out int intValue) is false)
        {
            return;
        }

        AddRangeFilter(query, fieldName, intValue, filterOperation);
    }

    private void ApplyRangeDateFilter(IQuery query, string fieldName, string[] values, FilterOperation filterOperation)
    {
        if (TryParseDateTimeFilterValue(values.First(), out DateTime dateValue) is false)
        {
            return;
        }

        AddRangeFilter(query, fieldName, dateValue, filterOperation);
    }

    private void AddRangeFilter<T>(IQuery query, string fieldName, T value, FilterOperation filterOperation)
        where T : struct
    {
        T? min = null, max = null;
        bool minInclusive = false, maxInclusive = false;

        switch (filterOperation)
        {
            case FilterOperation.GreaterThan:
            case FilterOperation.GreaterThanOrEqual:
                min = value;
                minInclusive = filterOperation is FilterOperation.GreaterThanOrEqual;
                break;
            case FilterOperation.LessThan:
            case FilterOperation.LessThanOrEqual:
                max = value;
                maxInclusive = filterOperation is FilterOperation.LessThanOrEqual;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(filterOperation));
        }

        query.RangeQuery(new[] { fieldName }, min, max, minInclusive, maxInclusive);
    }

    private void AddGroupedOrFilter<T>(IQuery query, string fieldName, params T[] values)
        where T : struct
    {
        if (values.Length == 0)
        {
            return;
        }

        var fields = new[] { fieldName };
        query.Group(
            nestedQuery =>
            {
                INestedBooleanOperation nestedQueryOperation =
                    nestedQuery.RangeQuery<T>(fields, values[0], values[0]);
                foreach (T value in values.Skip(1))
                {
                    nestedQueryOperation = nestedQueryOperation.Or().RangeQuery<T>(fields, value, value);
                }

                return nestedQueryOperation;
            });
    }

    private void AddGroupedOrFilter(IQuery query, string fieldName, params string[] values)
        => query.GroupedOr(new[] { fieldName }, values);

    private bool TryParseIntFilterValue(string value, out int intValue)
        => int.TryParse(value, out intValue);

    private bool TryParseDateTimeFilterValue(string value, out DateTime dateValue)
        => DateTime.TryParse(value, out dateValue);
}
