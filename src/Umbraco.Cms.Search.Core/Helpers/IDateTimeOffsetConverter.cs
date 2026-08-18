namespace Umbraco.Cms.Search.Core.Helpers;

/// <summary>
/// Converts <see cref="DateTime"/> values to <see cref="DateTimeOffset"/> for indexing and filtering.
/// </summary>
public interface IDateTimeOffsetConverter
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> to a <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="dateTime">The date/time to convert.</param>
    /// <returns>The converted date/time offset.</returns>
    DateTimeOffset ToDateTimeOffset(DateTime dateTime);
}
