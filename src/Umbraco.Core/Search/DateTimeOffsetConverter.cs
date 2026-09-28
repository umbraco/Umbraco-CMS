namespace Umbraco.Cms.Core.Search;

/// <summary>
/// Converts <see cref="DateTime"/> values to <see cref="DateTimeOffset"/> using a zero UTC offset.
/// </summary>
public sealed class DateTimeOffsetConverter : IDateTimeOffsetConverter
{
    /// <inheritdoc />
    public DateTimeOffset ToDateTimeOffset(DateTime dateTime)
        => new(
            new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day),
            new TimeOnly(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, dateTime.Microsecond),
            TimeSpan.Zero
        );
}
