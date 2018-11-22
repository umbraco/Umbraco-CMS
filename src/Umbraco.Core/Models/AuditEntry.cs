using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an audited event.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class AuditEntry : Entity, IAuditEntry
    {
        private static PropertySelectors _selectors;

        private int _performingUserId;
        private string _performingDetails;
        private string _performingIp;
        private int _affectedUserId;
        private string _affectedDetails;
        private string _eventType;
        private string _eventDetails;

        private static PropertySelectors Selectors => _selectors ?? (_selectors = new PropertySelectors());

        private class PropertySelectors
        {
            public readonly PropertyInfo PerformingUserId = ExpressionHelper.GetPropertyInfo<AuditEntry, int>(x => x.PerformingUserId);
            public readonly PropertyInfo PerformingDetails = ExpressionHelper.GetPropertyInfo<AuditEntry, string>(x => x.PerformingDetails);
            public readonly PropertyInfo PerformingIp = ExpressionHelper.GetPropertyInfo<AuditEntry, string>(x => x.PerformingIp);
            public readonly PropertyInfo AffectedUserId = ExpressionHelper.GetPropertyInfo<AuditEntry, int>(x => x.AffectedUserId);
            public readonly PropertyInfo AffectedDetails = ExpressionHelper.GetPropertyInfo<AuditEntry, string>(x => x.AffectedDetails);
            public readonly PropertyInfo EventType = ExpressionHelper.GetPropertyInfo<AuditEntry, string>(x => x.EventType);
            public readonly PropertyInfo EventDetails = ExpressionHelper.GetPropertyInfo<AuditEntry, string>(x => x.EventDetails);
        }

        /// <inheritdoc />
        public int PerformingUserId
        {
            get => _performingUserId;
            set => SetPropertyValueAndDetectChanges(value, ref _performingUserId, Selectors.PerformingUserId);
        }

        /// <inheritdoc />
        public string PerformingDetails
        {
            get => _performingDetails;
            set => SetPropertyValueAndDetectChanges(value, ref _performingDetails, Selectors.PerformingDetails);
        }

        /// <inheritdoc />
        public string PerformingIp
        {
            get => _performingIp;
            set => SetPropertyValueAndDetectChanges(value, ref _performingIp, Selectors.PerformingIp);
        }

        /// <inheritdoc />
        public DateTime EventDateUtc
        {
            get => CreateDate;
            set => CreateDate = value;
        }

        /// <inheritdoc />
        public int AffectedUserId 
        {
            get => _affectedUserId;
            set => SetPropertyValueAndDetectChanges(value, ref _affectedUserId, Selectors.AffectedUserId);
        }

        /// <inheritdoc />
        public string AffectedDetails 
        {
            get => _affectedDetails;
            set => SetPropertyValueAndDetectChanges(value, ref _affectedDetails, Selectors.AffectedDetails);
        }

        /// <inheritdoc />
        public string EventType 
        {
            get => _eventType;
            set => SetPropertyValueAndDetectChanges(value, ref _eventType, Selectors.EventType);
        }

        /// <inheritdoc />
        public string EventDetails 
        {
            get => _eventDetails;
            set => SetPropertyValueAndDetectChanges(value, ref _eventDetails, Selectors.EventDetails);
        }
    }
}
