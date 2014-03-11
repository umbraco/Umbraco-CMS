using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Exposes a member object as IPublishedContent
    /// </summary>
    internal class MemberPublishedContent : PublishedContentBase
    {

        private readonly IMember _member;
        private readonly MembershipUser _membershipUser;
        private readonly List<IPublishedProperty> _properties;
        private readonly PublishedContentType _publishedMemberType;

        public MemberPublishedContent(IMember member, MembershipUser membershipUser)
        {
            if (member == null) throw new ArgumentNullException("member");
            if (membershipUser == null) throw new ArgumentNullException("membershipUser");

            _member = member;
            _membershipUser = membershipUser;
            _properties = new List<IPublishedProperty>();
            _publishedMemberType = PublishedContentType.Get(PublishedItemType.Member, _member.ContentTypeAlias);
            if (_publishedMemberType == null)
            {
                throw new InvalidOperationException("Could not get member type with alias " + _member.ContentTypeAlias);
            }
            foreach (var propType in _publishedMemberType.PropertyTypes)
            {
                var val = _member.Properties.Any(x => x.Alias == propType.PropertyTypeAlias) == false
                    ? string.Empty 
                    : _member.Properties[propType.PropertyTypeAlias].Value;
                _properties.Add(new RawValueProperty(propType, val ?? string.Empty));
            }
        }

        #region Membership provider member properties
        public string Email
        {
            get { return _membershipUser.Email; }
        }
        public string UserName
        {
            get { return _membershipUser.UserName; }
        }
        public string PasswordQuestion
        {
            get { return _membershipUser.PasswordQuestion; }
        }
        public string Comments
        {
            get { return _membershipUser.Comment; }
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
            get { return _membershipUser.CreationDate; }
        }
        public DateTime LastLoginDate
        {
            get { return _membershipUser.LastLoginDate; }
        }
        public DateTime LastActivityDate
        {
            get { return _membershipUser.LastActivityDate; }
        }
        public DateTime LastPasswordChangedDate
        {
            get { return _membershipUser.LastPasswordChangedDate; }
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
            get { return _member.GetCreatorProfile().Name; }
        }

        public override string CreatorName
        {
            get { return _member.GetCreatorProfile().Name; }
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
