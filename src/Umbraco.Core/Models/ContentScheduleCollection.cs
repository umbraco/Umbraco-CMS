﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Umbraco.Core.Models
{
    public class ContentScheduleCollection : INotifyCollectionChanged, IDeepCloneable, IEquatable<ContentScheduleCollection>
    {
        //underlying storage for the collection backed by a sorted list so that the schedule is always in order of date and that duplicate dates per culture are not allowed
        private readonly Dictionary<string, SortedList<DateTime, ContentSchedule>> _schedule
            = new Dictionary<string, SortedList<DateTime, ContentSchedule>>(StringComparer.InvariantCultureIgnoreCase);

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Clears all <see cref="CollectionChanged"/> event handlers
        /// </summary>
        public void ClearCollectionChangedEvents() => CollectionChanged = null;

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

            // TODO: Below will throw if there are duplicate dates added, validate/return bool?
            changes.Add(schedule.Date, schedule);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, schedule));
        }

        /// <summary>
        /// Adds a new schedule for invariant content
        /// </summary>
        /// <param name="releaseDate"></param>
        /// <param name="expireDate"></param>
        public bool Add(DateTime? releaseDate, DateTime? expireDate)
        {
            return Add(string.Empty, releaseDate, expireDate);
        }

        /// <summary>
        /// Adds a new schedule for a culture
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="releaseDate"></param>
        /// <param name="expireDate"></param>
        /// <returns>true if successfully added, false if validation fails</returns>
        public bool Add(string culture, DateTime? releaseDate, DateTime? expireDate)
        {
            if (culture == null) throw new ArgumentNullException(nameof(culture));
            if (releaseDate.HasValue && expireDate.HasValue && releaseDate >= expireDate)
                return false;

            if (!releaseDate.HasValue && !expireDate.HasValue) return false;

            // TODO: Do we allow passing in a release or expiry date that is before now?

            if (!_schedule.TryGetValue(culture, out var changes))
            {
                changes = new SortedList<DateTime, ContentSchedule>();
                _schedule[culture] = changes;
            }

            // TODO: Below will throw if there are duplicate dates added, should validate/return bool?
            // but the bool won't indicate which date was in error, maybe have 2 diff methods to schedule start/end?

            if (releaseDate.HasValue)
            {
                var entry = new ContentSchedule(culture, releaseDate.Value, ContentScheduleAction.Release);
                changes.Add(releaseDate.Value, entry);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entry));
            }

            if (expireDate.HasValue)
            {
                var entry = new ContentSchedule(culture, expireDate.Value, ContentScheduleAction.Expire);
                changes.Add(expireDate.Value, entry);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entry));
            }

            return true;
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
        /// <param name="action"></param>
        /// <param name="changeDate">If specified, will clear all entries with dates less than or equal to the value</param>
        public void Clear(ContentScheduleAction action, DateTime? changeDate = null)
        {
            Clear(string.Empty, action, changeDate);
        }

        /// <summary>
        /// Clear all of the scheduled change type for the culture
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="action"></param>
        /// <param name="date">If specified, will clear all entries with dates less than or equal to the value</param>
        public void Clear(string culture, ContentScheduleAction action, DateTime? date = null)
        {
            if (!_schedule.TryGetValue(culture, out var schedules))
                return;

            var removes = schedules.Where(x => x.Value.Action == action && (!date.HasValue || x.Value.Date <= date.Value)).ToList();

            foreach (var remove in removes)
            {
                var removed = schedules.Remove(remove.Value.Date);
                if (!removed)
                    continue;

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, remove.Value));
            }

            if (schedules.Count == 0)
                _schedule.Remove(culture);
        }

        /// <summary>
        /// Returns all pending schedules based on the date and type provided
        /// </summary>
        /// <param name="action"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public IReadOnlyList<ContentSchedule> GetPending(ContentScheduleAction action, DateTime date)
        {
            return _schedule.Values.SelectMany(x => x.Values).Where(x => x.Date <= date).ToList();
        }

        /// <summary>
        /// Gets the schedule for invariant content
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ContentSchedule> GetSchedule(ContentScheduleAction? action = null)
        {
            return GetSchedule(string.Empty, action);
        }

        /// <summary>
        /// Gets the schedule for a culture
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public IEnumerable<ContentSchedule> GetSchedule(string culture, ContentScheduleAction? action = null)
        {
            if (_schedule.TryGetValue(culture, out var changes))
                return action == null ? changes.Values : changes.Values.Where(x => x.Action == action.Value);
            return Enumerable.Empty<ContentSchedule>();
        }

        /// <summary>
        /// Returns all schedules registered
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<ContentSchedule> FullSchedule => _schedule.SelectMany(x => x.Value.Values).ToList();

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
            => obj is ContentScheduleCollection other && Equals(other);

        public bool Equals(ContentScheduleCollection other)
        {
            if (other == null) return false;

            var thisSched = _schedule;
            var thatSched = other._schedule;

            if (thisSched.Count != thatSched.Count)
                return false;

            foreach (var (culture, thisList) in thisSched)
            {
                // if culture is missing, or actions differ, false
                if (!thatSched.TryGetValue(culture, out var thatList) || !thatList.SequenceEqual(thisList))
                    return false;
            }

            return true;
        }
    }
}
