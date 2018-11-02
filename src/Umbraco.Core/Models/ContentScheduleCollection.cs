using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Umbraco.Core.Models
{
    public class ContentScheduleCollection
    {
        //underlying storage for the collection backed by a sorted list so that the schedule is always in order of date
        private readonly Dictionary<string, SortedList<DateTime, ContentSchedule>> _schedule = new Dictionary<string, SortedList<DateTime, ContentSchedule>>();

        /// <summary>
        /// Add an existing schedule
        /// </summary>
        /// <param name="schedule"></param>
        public void Add(ContentSchedule schedule)
        {
            if (!_schedule.TryGetValue(schedule.Culture, out var changes))
            {
                changes = new SortedList<DateTime, ContentSchedule>();
                _schedule[schedule.Culture] = changes;
            }

            //TODO: Below will throw if there are duplicate dates added, validate/return bool?
            changes.Add(schedule.Date, schedule);
        }

        /// <summary>
        /// Adds a new schedule for invariant content
        /// </summary>
        /// <param name="releaseDate"></param>
        /// <param name="expireDate"></param>
        public void Add(DateTime? releaseDate, DateTime? expireDate)
        {
            Add(string.Empty, releaseDate, expireDate);
        }

        /// <summary>
        /// Adds a new schedule for a culture
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="releaseDate"></param>
        /// <param name="expireDate"></param>
        public void Add(string culture, DateTime? releaseDate, DateTime? expireDate)
        {
            if (culture == null) throw new ArgumentNullException(nameof(culture));
            if (releaseDate.HasValue && expireDate.HasValue && releaseDate >= expireDate)
                throw new InvalidOperationException($"The {nameof(releaseDate)} must be less than {nameof(expireDate)}");

            if (!releaseDate.HasValue && !expireDate.HasValue) return;

            //TODO: Do we allow passing in a release or expiry date that is before now?

            if (!_schedule.TryGetValue(culture, out var changes))
            {
                changes = new SortedList<DateTime, ContentSchedule>();
                _schedule[culture] = changes;
            }   

            //TODO: Below will throw if there are duplicate dates added, should validate/return bool?
            // but the bool won't indicate which date was in error, maybe have 2 diff methods to schedule start/end?

            if (releaseDate.HasValue)
                changes.Add(releaseDate.Value, new ContentSchedule(0, culture, releaseDate.Value, ContentScheduleChange.Start));
            if (expireDate.HasValue)
                changes.Add(expireDate.Value, new ContentSchedule(0, culture, expireDate.Value, ContentScheduleChange.End));
            
        }

        /// <summary>
        /// Remove a scheduled change
        /// </summary>
        /// <param name="change"></param>
        public void Remove(ContentSchedule change)
        {
            if (_schedule.TryGetValue(change.Culture, out var s))
            {
                s.Remove(change.Date);
            }
        }

        /// <summary>
        /// Clear all of the scheduled change type for invariant content
        /// </summary>
        /// <param name="changeType"></param>
        public void Clear(ContentScheduleChange changeType)
        {
            Clear(string.Empty, changeType);
        }

        /// <summary>
        /// Clear all of the scheduled change type for the culture
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="changeType"></param>
        public void Clear(string culture, ContentScheduleChange changeType)
        {
            if (_schedule.TryGetValue(culture, out var s))
            {
                foreach (var ofChange in s.Where(x => x.Value.Change == changeType).ToList())
                    s.Remove(ofChange.Value.Date);
            }
        }

        /// <summary>
        /// Gets the schedule for invariant content
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ContentSchedule> GetSchedule(ContentScheduleChange? changeType = null)
        {
            return GetSchedule(string.Empty, changeType);
        }

        /// <summary>
        /// Gets the schedule for a culture
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public IEnumerable<ContentSchedule> GetSchedule(string culture, ContentScheduleChange? changeType = null)
        {
            if (_schedule.TryGetValue(culture, out var changes))
                return changeType == null ? changes.Values : changes.Values.Where(x => x.Change == changeType.Value);
            return null;
        }

        /// <summary>
        /// Returns all schedules for both invariant and variant cultures
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, IEnumerable<ContentSchedule>> GetFullSchedule()
        {
            return _schedule.ToDictionary(x => x.Key, x => (IEnumerable<ContentSchedule>)x.Value.Values);
        }
    }
}
