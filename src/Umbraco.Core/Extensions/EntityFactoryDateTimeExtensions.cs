namespace Umbraco.Cms.Core.Extensions;

/// <summary>
/// Provides extensions on <see cref="DateTime"/> purely when building entities from persistence DTOs.
/// </summary>
public static class EntityFactoryDateTimeExtensions
{
    /// <summary>
    /// Ensures the provided DateTime is in UTC format.
    /// </summary>
    /// <remarks>
    /// We need this in the particular cases of building entities from persistence DTOs. NPoco isn't consistent in what it returns
    /// here across databases, sometimes providing a Kind of Unspecified. We are consistently persisting UTC for Umbraco's system
    /// dates so we should enforce this Kind on the entity before exposing it further within the Umbraco application.
    /// </remarks>
    public static DateTime EnsureUtc(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
        {
            return dateTime;
        }

        if (dateTime.Kind == DateTimeKind.Local)
        {
            return dateTime.ToUniversalTime();
        }

        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}
