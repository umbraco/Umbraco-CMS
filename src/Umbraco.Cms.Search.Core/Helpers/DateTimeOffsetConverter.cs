namespace Umbraco.Cms.Search.Core.Helpers;

internal sealed class DateTimeOffsetConverter : IDateTimeOffsetConverter
{
    // NOTE: in V15 this can be done using dateTime.TryConvert<DateTimeOffset>()
    public DateTimeOffset ToDateTimeOffset(DateTime dateTime)
        => new(
            new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day),
            new TimeOnly(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, dateTime.Microsecond),
            TimeSpan.Zero
        );
}
