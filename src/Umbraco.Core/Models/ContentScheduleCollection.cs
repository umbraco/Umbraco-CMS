using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Umbraco.Core.Models
{
    public class ContentScheduleCollection : INotifyCollectionChanged, IDeepCloneable, IEquatable<ContentScheduleCollection>
    {
        //underlying storage for the collection backed by a sorted list so that the schedule is always in order of date
        private readonly Dictionary<string, SortedList<DateTime, ContentSchedule>> _schedule
            = new Dictionary<string, SortedList<DateTime, ContentSchedule>>(StringComparer.InvariantCultureIgnoreCase);

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

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

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, schedule));
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
            {
                var entry = new ContentSchedule(0, culture, releaseDate.Value, ContentScheduleChange.Start);
                changes.Add(releaseDate.Value, entry);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entry));
            }
                
            if (expireDate.HasValue)
            {
                var entry = new ContentSchedule(0, culture, expireDate.Value, ContentScheduleChange.End);
                changes.Add(expireDate.Value, entry);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entry));
            }
        }

        /// <summary>
        /// Remove a scheduled change
        /// </summary>
        /// <param name="change"></param>
        public void Remove(ContentSchedule change)
        {
            if (_schedule.TryGetValue(change.Culture, out var s))
            {
                var removed = s.Remove(change.Date);
                if (removed)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, change));
                    if (s.Count == 0)
                        _schedule.Remove(change.Culture);
                }
                    
            }
        }

        /// <summary>
        /// Clear all of the scheduled change type for invariant content
        /// </summary>
        /// <param name="changeType"></param>
        /// <param name="changeDate">If specified, will clear all entries with dates less than or equal to the value</param>
        public void Clear(ContentScheduleChange changeType, DateTime? changeDate = null)
        {
            Clear(string.Empty, changeType, changeDate);
        }

        /// <summary>
        /// Clear all of the scheduled change type for the culture
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="changeType"></param>
        /// <param name="changeDate">If specified, will clear all entries with dates less than or equal to the value</param>
        public void Clear(string culture, ContentScheduleChange changeType, DateTime? changeDate = null)
        {
            if (_schedule.TryGetValue(culture, out var s))
            {
                foreach (var ofChange in s.Where(x => x.Value.Change == changeType
                    && (changeDate.HasValue ? x.Value.Date <= changeDate.Value : true)).ToList())
                {
                    var removed = s.Remove(ofChange.Value.Date);
                    if (removed)
                    {
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, ofChange.Value));
                        if (s.Count == 0)
                            _schedule.Remove(culture);
                    }   
                }
                    
            }
        }

        /// <summary>
        /// Returns all pending schedules based on the date and type provided
        /// </summary>
        /// <param name="changeType"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public IEnumerable<ContentSchedule> GetPending(ContentScheduleChange changeType, DateTime date)
        {
            if (_schedule.TryGetValue(string.Empty, out var changes))
                return changes.Values.Where(x => x.Date <= date);
            return Enumerable.Empty<ContentSchedule>();
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
            return Enumerable.Empty<ContentSchedule>();
        }

        //fixme - should this just return IEnumerable<ContentSchedule> since the culture is part of the ContentSchedule object already?
        /// <summary>
        /// Returns all schedules for both invariant and variant cultures
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, IEnumerable<ContentSchedule>> FullSchedule => _schedule.ToDictionary(x => x.Key, x => (IEnumerable<ContentSchedule>)x.Value.Values);
        //public IEnumerable<ContentSchedule> FullSchedule => _schedule.SelectMany(x => x.Value.Values);

        public object DeepClone()
        {
            var clone = new ContentScheduleCollection();
            foreach(var cultureSched in _schedule)
            {
                var list = new SortedList<DateTime, ContentSchedule>();
                foreach (var schedEntry in cultureSched.Value)
                    list.Add(schedEntry.Key, (ContentSchedule)schedEntry.Value.DeepClone());
                clone._schedule[cultureSched.Key] = list;
            }
            return clone;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ContentScheduleCollection c))
                return false;
            return Equals(c);
        }

        public bool Equals(ContentScheduleCollection other)
        {
            var thisSched = this.FullSchedule;
            var thatSched = other.FullSchedule;

            var equal = false;
            if (thisSched.Count == thatSched.Count)
            {
                equal = true;
                foreach (var pair in thisSched)
                {
                    if (thatSched.TryGetValue(pair.Key, out var val))
                    {
                        if (val.SequenceEqual(pair.Value))
                        {
                            equal = false;
                            break;
                        }
                    }
                    else
                    {
                        // Require key be present.
                        equal = false;
                        break;
                    }
                }
            }
            return equal;
        }
        
    }
}
