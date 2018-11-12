using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a scheduled action for a document.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class ContentSchedule : IDeepCloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSchedule"/> class.
        /// </summary>
        public ContentSchedule(int id, string culture, DateTime date, ContentScheduleChange change)
        {
            Id = id;
            Culture = culture;
            Date = date;
            Change = change;
        }

        /// <summary>
        /// Gets the unique identifier of the document targeted by the scheduled action.
        /// </summary>
        [DataMember]
        public int Id { get; }

        /// <summary>
        /// Gets the culture of the scheduled action.
        /// </summary>
        /// <remarks>
        /// string.Empty represents the invariant culture.
        /// </remarks>
        [DataMember]
        public string Culture { get; }

        /// <summary>
        /// Gets the date of the scheduled action.
        /// </summary>
        [DataMember]
        public DateTime Date { get; }

        /// <summary>
        /// Gets the action to take.
        /// </summary>
        [DataMember]
        public ContentScheduleChange Change { get; }

        // fixme/review - must implement Equals?
        //  fixing ContentScheduleCollection.Equals which was broken, breaks content Can_Deep_Clone test
        //  because SequenceEqual on the inner sorted lists fails, because it ends up doing reference-equal
        //  on each content schedule - so we *have* to implement Equals for us too?
        public override bool Equals(object obj)
            => obj is ContentSchedule other && Equals(other);

        public bool Equals(ContentSchedule other)
        {
            // don't compare Ids, two ContentSchedule are equal if they are for the same change
            // for the same culture, on the same date - and the collection deals w/duplicates
            return Culture.InvariantEquals(other.Culture) && Date == other.Date && Change == other.Change;
        }

        public object DeepClone()
        {
            return new ContentSchedule(Id, Culture, Date, Change);
        }
    }
}
