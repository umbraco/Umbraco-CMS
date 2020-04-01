using System;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class MemberBuilder
        : BuilderBase<Member>,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithCreatorIdBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder,
            IWithNameBuilder,
            IWithTrashedBuilder,
            IWithLevelBuilder,
            IWithPathBuilder,
            IWithSortOrderBuilder
    {
        private MemberTypeBuilder _memberTypeBuilder;
        private GenericCollectionBuilder<MemberBuilder, string> _memberGroupsBuilder;
        private GenericDictionaryBuilder<MemberBuilder, string, object> _additionalDataBuilder;
        private GenericDictionaryBuilder<MemberBuilder, string, object> _propertyDataBuilder;

        private int? _id;
        private Guid? _key;
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private string _name;
        private int? _creatorId;
        private string _username;
        private string _rawPasswordValue;
        private string _email;
        private int? _failedPasswordAttempts;
        private int? _level;
        private string _path;
        private bool? _isApproved;
        private bool? _isLockedOut;
        private DateTime? _lastLockoutDate;
        private DateTime? _lastLoginDate;
        private DateTime? _lastPasswordChangeDate;
        private int? _sortOrder;
        private bool? _trashed;
        private int? _propertyIdsIncrementingFrom;

        public MemberBuilder WithUserName(string username)
        {
            _username = username;
            return this;
        }

        public MemberBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public MemberBuilder WithRawPasswordValue(string rawPasswordValue)
        {
            _rawPasswordValue = rawPasswordValue;
            return this;
        }

        public MemberBuilder WithFailedPasswordAttempts(int failedPasswordAttempts)
        {
            _failedPasswordAttempts = failedPasswordAttempts;
            return this;
        }

        public MemberBuilder WithIsApproved(bool isApproved)
        {
            _isApproved = isApproved;
            return this;
        }

        public MemberBuilder WithIsLockedOut(bool isLockedOut)
        {
            _isLockedOut = isLockedOut;
            return this;
        }

        public MemberBuilder WithLastLockoutDate(DateTime lastLockoutDate)
        {
            _lastLockoutDate = lastLockoutDate;
            return this;
        }

        public MemberBuilder WithLastLoginDate(DateTime lastLoginDate)
        {
            _lastLoginDate = lastLoginDate;
            return this;
        }

        public MemberBuilder WithLastPasswordChangeDate(DateTime lastPasswordChangeDate)
        {
            _lastPasswordChangeDate = lastPasswordChangeDate;
            return this;
        }

        public MemberBuilder WithPropertyIdsIncrementingFrom(int propertyIdsIncrementingFrom)
        {
            _propertyIdsIncrementingFrom = propertyIdsIncrementingFrom;
            return this;
        }

        public MemberTypeBuilder AddMemberType()
        {
            var builder = new MemberTypeBuilder(this);
            _memberTypeBuilder = builder;
            return builder;
        }

        public GenericCollectionBuilder<MemberBuilder, string> AddMemberGroups()
        {
            var builder = new GenericCollectionBuilder<MemberBuilder, string>(this);
            _memberGroupsBuilder = builder;
            return builder;
        }

        public GenericDictionaryBuilder<MemberBuilder, string, object> AddAdditionalData()
        {
            var builder = new GenericDictionaryBuilder<MemberBuilder, string, object>(this);
            _additionalDataBuilder = builder;
            return builder;
        }

        public GenericDictionaryBuilder<MemberBuilder, string, object> AddPropertyData()
        {
            var builder = new GenericDictionaryBuilder<MemberBuilder, string, object>(this);
            _propertyDataBuilder = builder;
            return builder;
        }

        public override Member Build()
        {
            var id = _id ?? 1;
            var key = _key ?? Guid.NewGuid();
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var name = _name ?? Guid.NewGuid().ToString();
            var creatorId = _creatorId ?? 1;
            var username = _username ?? string.Empty;
            var email = _email ?? string.Empty;
            var rawPasswordValue = _rawPasswordValue ?? string.Empty;
            var failedPasswordAttempts = _failedPasswordAttempts ?? 0;
            var level = _level ?? 1;
            var path = _path ?? "-1";
            var isApproved = _isApproved ?? false;
            var isLockedOut = _isLockedOut ?? false;
            var lastLockoutDate = _lastLockoutDate ?? DateTime.Now;
            var lastLoginDate = _lastLoginDate ?? DateTime.Now;
            var lastPasswordChangeDate = _lastPasswordChangeDate ?? DateTime.Now;
            var sortOrder = _sortOrder ?? 0;
            var trashed = _trashed ?? false;

            if (_memberTypeBuilder == null)
            {
                throw new InvalidOperationException("A member cannot be constructed without providing a member type (use AddMemberType).");
            }

            var memberType = _memberTypeBuilder.Build();

            var member = new Member(name, email, username, rawPasswordValue, memberType)
            {
                Id = id,
                Key = key,
                CreateDate = createDate,
                UpdateDate = updateDate,
                CreatorId = creatorId,
                Level = level,
                Path = path,
                SortOrder = sortOrder,
                Trashed = trashed,
            };

            if (_propertyIdsIncrementingFrom.HasValue)
            {
                var i = _propertyIdsIncrementingFrom.Value;
                foreach (var property in member.Properties)
                {
                    property.Id = ++i;
                }
            }

            member.FailedPasswordAttempts = failedPasswordAttempts;
            member.IsApproved = isApproved;
            member.IsLockedOut = isLockedOut;
            member.LastLockoutDate = lastLockoutDate;
            member.LastLoginDate = lastLoginDate;
            member.LastPasswordChangeDate = lastPasswordChangeDate;

            if (_memberGroupsBuilder != null)
            {
                member.Groups = _memberGroupsBuilder.Build();
            }

            if (_additionalDataBuilder != null)
            {
                var additionalData = _additionalDataBuilder.Build();
                foreach (var kvp in additionalData)
                {
                    member.AdditionalData.Add(kvp.Key, kvp.Value);
                }
            }

            if (_propertyDataBuilder != null)
            {
                var propertyData = _propertyDataBuilder.Build();
                foreach (var kvp in propertyData)
                {
                    member.SetValue(kvp.Key, kvp.Value);
                }

                member.ResetDirtyProperties(false);
            }

            return member;
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

        int? IWithCreatorIdBuilder.CreatorId
        {
            get => _creatorId;
            set => _creatorId = value;
        }

        DateTime? IWithCreateDateBuilder.CreateDate
        {
            get => _createDate;
            set => _createDate = value;
        }

        DateTime? IWithUpdateDateBuilder.UpdateDate
        {
            get => _updateDate;
            set => _updateDate = value;
        }

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }

        bool? IWithTrashedBuilder.Trashed
        {
            get => _trashed;
            set => _trashed = value;
        }

        int? IWithLevelBuilder.Level
        {
            get => _level;
            set => _level = value;
        }

        string IWithPathBuilder.Path
        {
            get => _path;
            set => _path = value;
        }

        int? IWithSortOrderBuilder.SortOrder
        {
            get => _sortOrder;
            set => _sortOrder = value;
        }
    }
}
