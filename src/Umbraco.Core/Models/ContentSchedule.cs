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
    /// <remarks>
    ///     The schedule is created without an identifier, which is assigned when it is persisted.
    /// </remarks>
    public ContentSchedule(string culture, DateTime date, ContentScheduleAction action)
        : this(culture, date, action, null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSchedule" /> class.
    /// </summary>
    /// <param name="culture">The culture of the scheduled action.</param>
    /// <param name="date">The date of the scheduled action.</param>
    /// <param name="action">The action to take.</param>
    /// <param name="userId">The identifier of the user scheduling the action, or <c>null</c> if not known.</param>
    /// <remarks>
    ///     The schedule is created without an identifier, which is assigned when it is persisted.
    /// </remarks>
    public ContentSchedule(string culture, DateTime date, ContentScheduleAction action, int? userId)
        : this(Guid.Empty, culture, date, action, userId)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSchedule" /> class.
    /// </summary>
    /// <param name="id">The unique identifier of the scheduled action.</param>
    /// <param name="culture">The culture of the scheduled action.</param>
    /// <param name="date">The date of the scheduled action.</param>
    /// <param name="action">The action to take.</param>
    public ContentSchedule(Guid id, string culture, DateTime date, ContentScheduleAction action)
        : this(id, culture, date, action, null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSchedule" /> class.
    /// </summary>
    /// <param name="id">The unique identifier of the scheduled action.</param>
    /// <param name="culture">The culture of the scheduled action.</param>
    /// <param name="date">The date of the scheduled action.</param>
    /// <param name="action">The action to take.</param>
    /// <param name="userId">The identifier of the user scheduling the action, or <c>null</c> if not known.</param>
    public ContentSchedule(Guid id, string culture, DateTime date, ContentScheduleAction action, int? userId)
    {
        Id = id;
        Culture = culture;
        Date = date;
        Action = action;
        UserId = userId;
    }

    /// <summary>
    ///     Gets or sets the unique identifier of the scheduled action.
    /// </summary>
    /// <remarks>
    ///     <see cref="Guid.Empty" /> until the scheduled action is persisted, when an identifier is assigned.
    /// </remarks>
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
    ///     Gets the identifier of the user who scheduled the action.
    /// </summary>
    /// <remarks>
    ///     The scheduled action is performed on behalf of this user. <c>null</c> when the user isn't known,
    ///     in which case the action is performed on behalf of the last user to save the content.
    /// </remarks>
    [DataMember]
    public int? UserId { get; }

    /// <inheritdoc />
    public object DeepClone() => new ContentSchedule(Id, Culture, Date, Action, UserId);

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
    ///     for the same culture, on the same date. The Id and UserId are not compared.
    /// </remarks>
    public bool Equals(ContentSchedule other) =>

        // don't compare Ids, two ContentSchedule are equal if they are for the same change
        // for the same culture, on the same date - and the collection deals w/duplicates
        // don't compare UserIds either, the user who scheduled the change describes the entry, not the change itself,
        // and user identifiers aren't shared between environments that may compare schedules
        Culture.InvariantEquals(other.Culture) && Date == other.Date && Action == other.Action;

    /// <inheritdoc />
    public override int GetHashCode() => throw new NotImplementedException();
}
