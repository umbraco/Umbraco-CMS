using System.Runtime.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a scheduled action for a document.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class ContentSchedule : IDeepCloneable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSchedule" /> class.
    /// </summary>
    /// <param name="culture">The culture of the scheduled action.</param>
    /// <param name="date">The date of the scheduled action.</param>
    /// <param name="action">The action to take.</param>
    /// <param name="includeInvariantForVariant">
    ///     Whether a scheduled release of variant content includes its invariant properties.
    /// </param>
    public ContentSchedule(string culture, DateTime date, ContentScheduleAction action, bool includeInvariantForVariant)
        : this(Guid.Empty, culture, date, action, includeInvariantForVariant)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSchedule" /> class.
    /// </summary>
    /// <param name="id">The unique identifier of the scheduled action.</param>
    /// <param name="culture">The culture of the scheduled action.</param>
    /// <param name="date">The date of the scheduled action.</param>
    /// <param name="action">The action to take.</param>
    /// <param name="includeInvariantForVariant">
    ///     Whether a scheduled release of variant content includes its invariant properties.
    /// </param>
    public ContentSchedule(Guid id, string culture, DateTime date, ContentScheduleAction action, bool includeInvariantForVariant)
    {
        Id = id; // Guid.Empty will be assigned by document repository
        Culture = culture;
        Date = date;
        Action = action;
        IncludeInvariantForVariant = includeInvariantForVariant;
    }

    /// <summary>
    ///     Gets the unique identifier of the document targeted by the scheduled action.
    /// </summary>
    [DataMember]
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets the culture of the scheduled action.
    /// </summary>
    /// <remarks>
    ///     string.Empty represents the invariant culture.
    /// </remarks>
    [DataMember]
    public string Culture { get; }

    /// <summary>
    ///     Gets the date of the scheduled action.
    /// </summary>
    [DataMember]
    public DateTime Date { get; }

    /// <summary>
    ///     Gets the action to take.
    /// </summary>
    [DataMember]
    public ContentScheduleAction Action { get; }

    /// <summary>
    ///     Gets a value indicating whether a scheduled release of variant content includes its invariant properties.
    /// </summary>
    /// <remarks>
    ///     Determined by the invariant-for-variant access of the user who scheduled the release.
    /// </remarks>
    [DataMember]
    public bool IncludeInvariantForVariant { get; }

    /// <inheritdoc />
    public object DeepClone() => new ContentSchedule(Id, Culture, Date, Action, IncludeInvariantForVariant);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is ContentSchedule other && Equals(other);

    /// <summary>
    ///     Determines whether this schedule is equal to another schedule.
    /// </summary>
    /// <param name="other">The other schedule to compare.</param>
    /// <returns><c>true</c> if the schedules are equal; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     Two ContentSchedule instances are equal if they are for the same action,
    ///     for the same culture, on the same date, including the same invariant properties. The Id is not compared.
    /// </remarks>
    public bool Equals(ContentSchedule other) =>

        // don't compare Ids, two ContentSchedule are equal if they are for the same change
        // for the same culture, on the same date - and the collection deals w/duplicates
        Culture.InvariantEquals(other.Culture) && Date == other.Date && Action == other.Action
        && IncludeInvariantForVariant == other.IncludeInvariantForVariant;

    /// <inheritdoc />
    public override int GetHashCode() => throw new NotImplementedException();
}
