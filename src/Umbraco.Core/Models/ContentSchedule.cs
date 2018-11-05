using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{

    /// <summary>
    /// Model for scheduled content
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class ContentSchedule : IDeepCloneable
    {
        public ContentSchedule(int id, string culture, DateTime date, ContentScheduleChange change)
        {
            Id = id;
            Culture = culture;
            Date = date;
            Change = change;
        }

        /// <summary>
        /// The unique Id of the schedule item
        /// </summary>
        [DataMember]
        public int Id { get; }

        /// <summary>
        /// The culture for the schedule item
        /// </summary>
        /// <remarks>
        /// string.Empty represents invariant culture
        /// </remarks>
        [DataMember]
        public string Culture { get; }

        /// <summary>
        /// The date for the schedule
        /// </summary>
        [DataMember]
        public DateTime Date { get; }

        /// <summary>
        /// The action to take for the schedule
        /// </summary>
        [DataMember]
        public ContentScheduleChange Change { get; }

        public object DeepClone()
        {
            return new ContentSchedule(Id, Culture, Date, Change);
        }
    }
}
