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
            _member = member ?? throw new ArgumentNullException(nameof(member));
            _membershipUser = member;
            _publishedMemberType = publishedMemberType ?? throw new ArgumentNullException(nameof(publishedMemberType));

            var properties = PublishedProperty.MapProperties(_publishedMemberType.PropertyTypes, _member.Properties,
                (t, v) => new RawValueProperty(t, this, v ?? string.Empty));
            _properties = WithMemberProperties(properties).ToArray();
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
            return _properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));
        }

        private IEnumerable<IPublishedProperty> WithMemberProperties(IEnumerable<IPublishedProperty> properties)
        {
            var propertiesList = properties.ToList();
            var aliases = propertiesList.Select(x => x.PropertyTypeAlias).ToList();

            if (!aliases.Contains("Email"))
                propertiesList.Add(new RawValueProperty(ContentType.GetPropertyType("Email"), this, Email));
            if (!aliases.Contains("UserName"))
                propertiesList.Add(new RawValueProperty(ContentType.GetPropertyType("UserName"), this, UserName));
            if (!aliases.Contains("PasswordQuestion"))
                propertiesList.Add(new RawValueProperty(ContentType.GetPropertyType("PasswordQuestion"), this, PasswordQuestion));
            if (!aliases.Contains("Comments"))
                propertiesList.Add(new RawValueProperty(ContentType.GetPropertyType("Comments"), this, Comments));
            if (!aliases.Contains("IsApproved"))
                propertiesList.Add(new RawValueProperty(ContentType.GetPropertyType("IsApproved"), this, IsApproved));
            if (!aliases.Contains("IsLockedOut"))
                propertiesList.Add(new RawValueProperty(ContentType.GetPropertyType("IsLockedOut"), this, IsLockedOut));
            if (!aliases.Contains("LastLockoutDate"))
                propertiesList.Add(new RawValueProperty(ContentType.GetPropertyType("LastLockoutDate"), this, LastLockoutDate));
            if (!aliases.Contains("CreateDate"))
                propertiesList.Add(new RawValueProperty(ContentType.GetPropertyType("CreateDate"), this, CreateDate));
            if (!aliases.Contains("LastLoginDate"))
                propertiesList.Add(new RawValueProperty(ContentType.GetPropertyType("LastLoginDate"), this, LastLoginDate));
            if (!aliases.Contains("LastPasswordChangeDate"))
                propertiesList.Add(new RawValueProperty(ContentType.GetPropertyType("LastPasswordChangeDate"), this, LastPasswordChangeDate));

            return propertiesList;
        }

        public override PublishedContentType ContentType => _publishedMemberType;

        public override int Id => _member.Id;

        public override Guid Key => _member.Key;

        public override int TemplateId => throw new NotSupportedException();

        public override int SortOrder => 0;

        public override string Name => _member.Name;

        public override string UrlName => throw new NotSupportedException();

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

        public override int Level => _member.Level;

        #endregion
    }
}
