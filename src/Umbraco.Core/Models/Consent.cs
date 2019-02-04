using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a consent.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class Consent : EntityBase, IConsent
    {
        private bool _current;
        private string _source;
        private string _context;
        private string _action;
        private ConsentState _state;
        private string _comment;

        /// <inheritdoc />
        public bool Current
        {
            get => _current;
            set => SetPropertyValueAndDetectChanges(value, ref _current, nameof(Current));
        }

        /// <inheritdoc />
        public string Source
        {
            get => _source;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(value));
                SetPropertyValueAndDetectChanges(value, ref _source, nameof(Source));
            }
        }

        /// <inheritdoc />
        public string Context
        {
            get => _context;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(value));
                SetPropertyValueAndDetectChanges(value, ref _context, nameof(Context));
            }
        }

        /// <inheritdoc />
        public string Action
        {
            get => _action;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(value));
                SetPropertyValueAndDetectChanges(value, ref _action, nameof(Action));
            }
        }

        /// <inheritdoc />
        public ConsentState State
        {
            get => _state;
            // note: we probably should validate the state here, but since the
            //  enum is [Flags] with many combinations, this could be expensive
            set => SetPropertyValueAndDetectChanges(value, ref _state, nameof(State));
        }

        /// <inheritdoc />
        public string Comment
        {
            get => _comment;
            set => SetPropertyValueAndDetectChanges(value, ref _comment, nameof(Comment));
        }

        /// <inheritdoc />
        public IEnumerable<IConsent> History => HistoryInternal;

        /// <summary>
        /// Gets the previous states of this consent.
        /// </summary>
        internal List<IConsent> HistoryInternal { get; set; }
    }
}
