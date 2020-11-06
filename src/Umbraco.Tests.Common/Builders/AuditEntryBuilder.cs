using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class AuditEntryBuilder : AuditEntryBuilder<object>
    {
        public AuditEntryBuilder() : base(null)
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

        public AuditEntryBuilder(TParent parentBuilder) : base(parentBuilder)
        {
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
            var key = _key ?? Guid.NewGuid();
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var deleteDate = _deleteDate ?? null;
            var affectedDetails = _affectedDetails ?? Guid.NewGuid().ToString();
            var affectedUserId = _affectedUserId ?? -1;
            var eventDetails = _eventDetails ?? Guid.NewGuid().ToString();
            var eventType = _eventType ?? "umbraco/user";
            var performingDetails = _performingDetails ?? Guid.NewGuid().ToString();
            var performingIp = _performingIp ?? "127.0.0.1";
            var eventDateUtc = _eventDateUtc ?? DateTime.UtcNow;
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
