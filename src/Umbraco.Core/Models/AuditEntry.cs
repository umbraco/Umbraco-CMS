using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an audited event.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class AuditEntry : EntityBase, IAuditEntry
    {
        private int _performingUserId;
        private string _performingDetails;
        private string _performingIp;
        private int _affectedUserId;
        private string _affectedDetails;
        private string _eventType;
        private string _eventDetails;

        /// <inheritdoc />
        public int PerformingUserId
        {
            get => _performingUserId;
            set => SetPropertyValueAndDetectChanges(value, ref _performingUserId, nameof(PerformingUserId));
        }

        /// <inheritdoc />
        public string PerformingDetails
        {
            get => _performingDetails;
            set => SetPropertyValueAndDetectChanges(value, ref _performingDetails, nameof(PerformingDetails));
        }

        /// <inheritdoc />
        public string PerformingIp
        {
            get => _performingIp;
            set => SetPropertyValueAndDetectChanges(value, ref _performingIp, nameof(PerformingIp));
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
            set => SetPropertyValueAndDetectChanges(value, ref _affectedUserId, nameof(AffectedUserId));
        }

        /// <inheritdoc />
        public string AffectedDetails
        {
            get => _affectedDetails;
            set => SetPropertyValueAndDetectChanges(value, ref _affectedDetails, nameof(AffectedDetails));
        }

        /// <inheritdoc />
        public string EventType
        {
            get => _eventType;
            set => SetPropertyValueAndDetectChanges(value, ref _eventType, nameof(EventType));
        }

        /// <inheritdoc />
        public string EventDetails
        {
            get => _eventDetails;
            set => SetPropertyValueAndDetectChanges(value, ref _eventDetails, nameof(EventDetails));
        }
    }
}
