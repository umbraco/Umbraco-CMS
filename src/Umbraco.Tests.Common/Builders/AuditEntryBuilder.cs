// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
{
    public class AuditEntryBuilder : AuditEntryBuilder<object>
    {
        public AuditEntryBuilder()
            : base(null)
        {
        }
    }

    public class AuditEntryBuilder<TParent>
        : ChildBuilderBase<TParent, IAuditEntry>,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder,
            IWithDeleteDateBuilder
    {
        private DateTime? _createDate;
        private DateTime? _deleteDate;
        private int? _id;
        private Guid? _key;
        private DateTime? _updateDate;
        private string _affectedDetails = null;
        private int? _affectedUserId = null;
        private string _eventDetails = null;
        private string _eventType = null;
        private string _performingDetails = null;
        private string _performingIp = null;
        private DateTime? _eventDateUtc = null;
        private int? _performingUserId = null;

        public AuditEntryBuilder(TParent parentBuilder)
            : base(parentBuilder)
        {
        }

        public AuditEntryBuilder<TParent> WithAffectedDetails(string affectedDetails)
        {
            _affectedDetails = affectedDetails;
            return this;
        }

        public AuditEntryBuilder<TParent> WithAffectedUserId(int affectedUserId)
        {
            _affectedUserId = affectedUserId;
            return this;
        }

        public AuditEntryBuilder<TParent> WithEventDetails(string eventDetails)
        {
            _eventDetails = eventDetails;
            return this;
        }

        public AuditEntryBuilder<TParent> WithEventType(string eventType)
        {
            _eventType = eventType;
            return this;
        }

        public AuditEntryBuilder<TParent> WithPerformingDetails(string performingDetails)
        {
            _performingDetails = performingDetails;
            return this;
        }

        public AuditEntryBuilder<TParent> WithPerformingIp(string performingIp)
        {
            _performingIp = performingIp;
            return this;
        }

        public AuditEntryBuilder<TParent> WithEventDate(DateTime eventDateUtc)
        {
            _eventDateUtc = eventDateUtc;
            return this;
        }

        public AuditEntryBuilder<TParent> WithPerformingUserId(int performingUserId)
        {
            _performingUserId = performingUserId;
            return this;
        }

        DateTime? IWithCreateDateBuilder.CreateDate
        {
            get => _createDate;
            set => _createDate = value;
        }

        DateTime? IWithDeleteDateBuilder.DeleteDate
        {
            get => _deleteDate;
            set => _deleteDate = value;
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }

        Guid? IWithKeyBuilder.Key
        {
            get => _key;
            set => _key = value;
        }

        DateTime? IWithUpdateDateBuilder.UpdateDate
        {
            get => _updateDate;
            set => _updateDate = value;
        }

        public override IAuditEntry Build()
        {
            var id = _id ?? 0;
            Guid key = _key ?? Guid.NewGuid();
            DateTime createDate = _createDate ?? DateTime.Now;
            DateTime updateDate = _updateDate ?? DateTime.Now;
            DateTime? deleteDate = _deleteDate ?? null;
            var affectedDetails = _affectedDetails ?? Guid.NewGuid().ToString();
            var affectedUserId = _affectedUserId ?? -1;
            var eventDetails = _eventDetails ?? Guid.NewGuid().ToString();
            var eventType = _eventType ?? "umbraco/user";
            var performingDetails = _performingDetails ?? Guid.NewGuid().ToString();
            var performingIp = _performingIp ?? "127.0.0.1";
            DateTime eventDateUtc = _eventDateUtc ?? DateTime.UtcNow;
            var performingUserId = _performingUserId ?? -1;

            return new AuditEntry
            {
                Id = id,
                Key = key,
                CreateDate = createDate,
                UpdateDate = updateDate,
                DeleteDate = deleteDate,
                AffectedDetails = affectedDetails,
                AffectedUserId = affectedUserId,
                EventDetails = eventDetails,
                EventType = eventType,
                PerformingDetails = performingDetails,
                PerformingIp = performingIp,
                EventDateUtc = eventDateUtc,
                PerformingUserId = performingUserId,
            };
        }
    }
}
