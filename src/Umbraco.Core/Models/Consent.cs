using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a consent.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class Consent : Entity, IConsent
    {
        private static PropertySelectors _selector;

        private string _source;
        private string _action;
        private string _actionType;
        private ConsentState _state;
        private string _comment;

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo Source = ExpressionHelper.GetPropertyInfo<Consent, string>(x => x.Source);
            public readonly PropertyInfo Action = ExpressionHelper.GetPropertyInfo<Consent, string>(x => x.Action);
            public readonly PropertyInfo ActionType = ExpressionHelper.GetPropertyInfo<Consent, string>(x => x.ActionType);
            public readonly PropertyInfo State = ExpressionHelper.GetPropertyInfo<Consent, ConsentState>(x => x.State);
            public readonly PropertyInfo Comment = ExpressionHelper.GetPropertyInfo<Consent, string>(x => x.Comment);
        }

        private static PropertySelectors Selectors => _selector ?? (_selector = new PropertySelectors());

        /// <inheritdoc />
        public string Source
        {
            get => _source;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(value));
                SetPropertyValueAndDetectChanges(value, ref _source, Selectors.Source);
            }
        }

        /// <inheritdoc />
        public string Action
        {
            get => _action;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(value));
                SetPropertyValueAndDetectChanges(value, ref _action, Selectors.Action);
            }
        }

        /// <inheritdoc />
        public string ActionType
        {
            get => _actionType;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(value));
                SetPropertyValueAndDetectChanges(value, ref _actionType, Selectors.ActionType);
            }
        }

        /// <inheritdoc />
        public ConsentState State
        {
            get => _state;
            // note: we probably should validate the state here, but since the
            //  enum is [Flags] with many combinations, this could be expensive
            set => SetPropertyValueAndDetectChanges(value, ref _state, Selectors.State);
        }

        public string Comment
        {
            get => _comment;
            set => SetPropertyValueAndDetectChanges(value, ref _comment, Selectors.Comment);
        }
    }
}
