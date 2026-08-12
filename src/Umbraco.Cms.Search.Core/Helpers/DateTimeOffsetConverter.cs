namespace Umbraco.Cms.Search.Core.Helpers;

/// <summary>
/// Converts <see cref="DateTime"/> values to <see cref="DateTimeOffset"/> using a zero UTC offset.
/// </summary>
internal sealed class DateTimeOffsetConverter : IDateTimeOffsetConverter
{
    // NOTE: in V15 this can be done using dateTime.TryConvert<DateTimeOffset>()
    /// <inheritdoc />
    public DateTimeOffset ToDateTimeOffset(DateTime dateTime)
        => new(
            new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day),
            new TimeOnly(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, dateTime.Microsecond),
            TimeSpan.Zero
        );
}
