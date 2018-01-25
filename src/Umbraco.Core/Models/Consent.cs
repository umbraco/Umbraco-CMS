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

        private Udi _source;
        private Udi _action;
        private ConsentState _state;
        private string _comment;

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo SourceUdi = ExpressionHelper.GetPropertyInfo<Consent, Udi>(x => x.Source);
            public readonly PropertyInfo ActionUdi = ExpressionHelper.GetPropertyInfo<Consent, Udi>(x => x.Action);
            public readonly PropertyInfo State = ExpressionHelper.GetPropertyInfo<Consent, ConsentState>(x => x.State);
            public readonly PropertyInfo Comment = ExpressionHelper.GetPropertyInfo<Consent, string>(x => x.Comment);
        }

        private static PropertySelectors Selectors => _selector ?? (_selector = new PropertySelectors());

        /// <inheritdoc />
        public Udi Source
        {
            get => _source;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                SetPropertyValueAndDetectChanges(value, ref _source, Selectors.SourceUdi);
            }
        }

        /// <inheritdoc />
        public Udi Action
        {
            get => _action;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                SetPropertyValueAndDetectChanges(value, ref _action, Selectors.ActionUdi);
            }
        }

        /// <inheritdoc />
        public string ActionType => _action.EntityType;

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
