using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Exposes a member object as IPublishedContent
    /// </summary>
    public sealed class PublishedMember : PublishedContentBase
    {
        private readonly IMember _member;
        private readonly IMembershipUser _membershipUser;
        private readonly IPublishedProperty[] _properties;
        private readonly PublishedContentType _publishedMemberType;

        public PublishedMember(IMember member, PublishedContentType publishedMemberType)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (publishedMemberType == null) throw new ArgumentNullException(nameof(publishedMemberType));

            _member = member;
            _membershipUser = member;
            _publishedMemberType = publishedMemberType;

            _properties = PublishedProperty.MapProperties(_publishedMemberType.PropertyTypes, _member.Properties,
                (t, v) => new RawValueProperty(t, v ?? string.Empty))
                .ToArray();
        }

        #region Membership provider member properties

        public string Email => _membershipUser.Email;

        public string UserName => _membershipUser.Username;

        public string PasswordQuestion => _membershipUser.PasswordQuestion;

        public string Comments => _membershipUser.Comments;

        public bool IsApproved => _membershipUser.IsApproved;

        public bool IsLockedOut => _membershipUser.IsLockedOut;

        public DateTime LastLockoutDate => _membershipUser.LastLockoutDate;

        public DateTime CreationDate => _membershipUser.CreateDate;

        public DateTime LastLoginDate => _membershipUser.LastLoginDate;

        public DateTime LastActivityDate => _membershipUser.LastLoginDate;

        public DateTime LastPasswordChangeDate => _membershipUser.LastPasswordChangeDate;

        #endregion

        #region IPublishedContent

        public override PublishedItemType ItemType => PublishedItemType.Member;

        public override bool IsDraft => false;

        public override IPublishedContent Parent => null;

        public override IEnumerable<IPublishedContent> Children => Enumerable.Empty<IPublishedContent>();

        public override IEnumerable<IPublishedProperty> Properties => _properties;

        public override IPublishedProperty GetProperty(string alias, bool recurse)
        {
            if (recurse)
            {
                throw new NotSupportedException();
            }
            return GetProperty(alias);
        }

        public override IPublishedProperty GetProperty(string alias)
        {
            switch (alias.ToLowerInvariant())
            {
                case "Email":
                    return new PropertyResult("Email", Email, PropertyResultType.CustomProperty);
                case "UserName":
                    return new PropertyResult("UserName", UserName, PropertyResultType.CustomProperty);
                case "PasswordQuestion":
                    return new PropertyResult("PasswordQuestion", PasswordQuestion, PropertyResultType.CustomProperty);
                case "Comments":
                    return new PropertyResult("Comments", Email, PropertyResultType.CustomProperty);
                case "IsApproved":
                    return new PropertyResult("IsApproved", IsApproved, PropertyResultType.CustomProperty);
                case "IsLockedOut":
                    return new PropertyResult("IsLockedOut", IsLockedOut, PropertyResultType.CustomProperty);
                case "LastLockoutDate":
                    return new PropertyResult("LastLockoutDate", LastLockoutDate, PropertyResultType.CustomProperty);
                case "CreateDate":
                    return new PropertyResult("CreateDate", CreateDate, PropertyResultType.CustomProperty);
                case "LastLoginDate":
                    return new PropertyResult("LastLoginDate", LastLoginDate, PropertyResultType.CustomProperty);
                case "LastPasswordChangeDate":
                    return new PropertyResult("LastPasswordChangeDate", LastPasswordChangeDate, PropertyResultType.CustomProperty);
            }

            return _properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));
        }

        public override PublishedContentType ContentType => _publishedMemberType;

        public override int Id => _member.Id;

        public override Guid Key => _member.Key;

        public override int TemplateId
        {
            get { throw new NotSupportedException(); }
        }

        public override int SortOrder => 0;

        public override string Name => _member.Name;

        public override string UrlName
        {
            get { throw new NotSupportedException(); }
        }

        public override string DocumentTypeAlias => _member.ContentTypeAlias;

        public override int DocumentTypeId => _member.ContentType.Id;

        //TODO: ARGH! need to fix this - this is not good because it uses ApplicationContext.Current
        public override string WriterName => _member.GetCreatorProfile().Name;

        //TODO: ARGH! need to fix this - this is not good because it uses ApplicationContext.Current
        public override string CreatorName => _member.GetCreatorProfile().Name;

        public override int WriterId => _member.CreatorId;

        public override int CreatorId => _member.CreatorId;

        public override string Path => _member.Path;

        public override DateTime CreateDate => _member.CreateDate;

        public override DateTime UpdateDate => _member.UpdateDate;

        public override Guid Version => _member.Version;

        public override int Level => _member.Level;

        #endregion
    }
}
