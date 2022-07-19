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
    public ContentSchedule(string culture, DateTime date, ContentScheduleAction action)
    {
        Id = Guid.Empty; // will be assigned by document repository
        Culture = culture;
        Date = date;
        Action = action;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentSchedule" /> class.
    /// </summary>
    public ContentSchedule(Guid id, string culture, DateTime date, ContentScheduleAction action)
    {
        Id = id;
        Culture = culture;
        Date = date;
        Action = action;
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

    public object DeepClone() => new ContentSchedule(Id, Culture, Date, Action);

    public override bool Equals(object? obj)
        => obj is ContentSchedule other && Equals(other);

    public bool Equals(ContentSchedule other) =>

        // don't compare Ids, two ContentSchedule are equal if they are for the same change
        // for the same culture, on the same date - and the collection deals w/duplicates
        Culture.InvariantEquals(other.Culture) && Date == other.Date && Action == other.Action;

    public override int GetHashCode() => throw new NotImplementedException();
}
