using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Exposes a member object as IPublishedContent
    /// </summary>
    public sealed class PublishedMember : PublishedContentWithKeyBase
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
        public string Email
        {
            get { return _membershipUser.Email; }
        }
        public string UserName
        {
            get { return _membershipUser.Username; }
        }
        public string PasswordQuestion
        {
            get { return _membershipUser.PasswordQuestion; }
        }
        public string Comments
        {
            get { return _membershipUser.Comments; }
        }
        public bool IsApproved
        {
            get { return _membershipUser.IsApproved; }
        }
        public bool IsLockedOut
        {
            get { return _membershipUser.IsLockedOut; }
        }
        public DateTime LastLockoutDate
        {
            get { return _membershipUser.LastLockoutDate; }
        }
        public DateTime CreationDate
        {
            get { return _membershipUser.CreateDate; }
        }
        public DateTime LastLoginDate
        {
            get { return _membershipUser.LastLoginDate; }
        }
        public DateTime LastActivityDate
        {
            get { return _membershipUser.LastLoginDate; }
        }
        public DateTime LastPasswordChangedDate
        {
            get { return _membershipUser.LastPasswordChangeDate; }
        }
        #endregion

        #region IPublishedContent
        public override PublishedItemType ItemType
        {
            get { return PublishedItemType.Member; }
        }

        public override bool IsDraft
        {
            get { return false; }
        }

        public override IPublishedContent Parent
        {
            get { return null; }
        }

        public override IEnumerable<IPublishedContent> Children
        {
            get { return Enumerable.Empty<IPublishedContent>(); }
        }

        public override ICollection<IPublishedProperty> Properties
        {
            get { return _properties; }
        }

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
            switch (alias)
            {
                case "Email":
                    return new PropertyResult("Email", Email, PropertyResultType.CustomProperty);
                case "UserName":
                    return new PropertyResult("UserName", UserName, PropertyResultType.CustomProperty);
            }

            return _properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));
        }

        public override PublishedContentType ContentType
        {
            get { return _publishedMemberType; }
        }

        public override int Id
        {
            get { return _member.Id; }
        }

        public override Guid Key
        {
            get { return _member.Key; }
        }

        public override int TemplateId
        {
            get { throw new NotSupportedException(); }
        }

        public override int SortOrder
        {
            get { return 0; }
        }

        public override string Name
        {
            get { return _member.Name; }
        }

        public override string UrlName
        {
            get { throw new NotSupportedException(); }
        }

        public override string DocumentTypeAlias
        {
            get { return _member.ContentTypeAlias; }
        }

        public override int DocumentTypeId
        {
            get { return _member.ContentType.Id; }
        }

        public override string WriterName
        {
            get
            {
                //TODO: ARGH! need to fix this - this is not good because it uses ApplicationContext.Current
                return _member.GetCreatorProfile().Name;
            }
        }

        public override string CreatorName
        {
            get
            {
                //TODO: ARGH! need to fix this - this is not good because it uses ApplicationContext.Current
                return _member.GetCreatorProfile().Name;
            }
        }

        public override int WriterId
        {
            get { return _member.CreatorId; }
        }

        public override int CreatorId
        {
            get { return _member.CreatorId; }
        }

        public override string Path
        {
            get { return _member.Path; }
        }

        public override DateTime CreateDate
        {
            get { return _member.CreateDate; }
        }

        public override DateTime UpdateDate
        {
            get { return _member.UpdateDate; }
        }

        public override Guid Version
        {
            get { return _member.Version; }
        }

        public override int Level
        {
            get { return _member.Level; }
        }
        #endregion
    }
}
