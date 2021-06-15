using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Tests.LegacyXmlPublishedCache
{
    /// <summary>
    /// Exposes a member object as IPublishedContent
    /// </summary>
    public sealed class PublishedMember : PublishedContentBase
    {
        private readonly IMember _member;
        private readonly IMembershipUser _membershipUser;
        private readonly IPublishedProperty[] _properties;
        private readonly IPublishedContentType _publishedMemberType;

        public PublishedMember(
            IMember member,
            IPublishedContentType publishedMemberType,
            IVariationContextAccessor variationContextAccessor) : base(variationContextAccessor)
        {
            _member = member ?? throw new ArgumentNullException(nameof(member));
            _membershipUser = member;
            _publishedMemberType = publishedMemberType ?? throw new ArgumentNullException(nameof(publishedMemberType));

            // RawValueProperty is used for two things here
            // - for the 'map properties' thing that we should really get rid of
            // - for populating properties that every member should always have, and that we force-create
            //   if they are not part of the member type properties - in which case they are created as
            //   simple raw properties - which are completely invariant

            var properties = new List<IPublishedProperty>();
            foreach (var propertyType in _publishedMemberType.PropertyTypes)
            {
                var property = _member.Properties[propertyType.Alias];
                if (property == null) continue;

                properties.Add(new RawValueProperty(propertyType, this, property.GetValue()));
            }
            EnsureMemberProperties(properties);
            _properties = properties.ToArray();
        }

        #region Membership provider member properties

        public string Email => _membershipUser.Email;

        public string UserName => _membershipUser.Username;

        public string Comments => _membershipUser.Comments;

        public bool IsApproved => _membershipUser.IsApproved;

        public bool IsLockedOut => _membershipUser.IsLockedOut;

        public DateTime LastLockoutDate => _membershipUser.LastLockoutDate;

        public DateTime CreationDate => _membershipUser.CreateDate;

        public DateTime LastLoginDate => _membershipUser.LastLoginDate;

        public DateTime LastPasswordChangeDate => _membershipUser.LastPasswordChangeDate;

        #endregion

        #region IPublishedContent

        public override PublishedItemType ItemType => PublishedItemType.Member;

        public override bool IsDraft(string culture = null) => false;

        public override bool IsPublished(string culture = null) => true;

        public override IPublishedContent Parent => null;

        public override IEnumerable<IPublishedContent> Children => Enumerable.Empty<IPublishedContent>();

        public override IEnumerable<IPublishedContent> ChildrenForAllCultures => Enumerable.Empty<IPublishedContent>();

        public override IEnumerable<IPublishedProperty> Properties => _properties;

        public override IPublishedProperty GetProperty(string alias)
        {
            return _properties.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        }

        private void EnsureMemberProperties(List<IPublishedProperty> properties)
        {
            var aliases = properties.Select(x => x.Alias).ToList();

            EnsureMemberProperty(properties, aliases, nameof(IMember.Email), Email);
            EnsureMemberProperty(properties, aliases, nameof(IMember.Username), UserName);
            EnsureMemberProperty(properties, aliases, nameof(IMember.Comments), Comments);
            EnsureMemberProperty(properties, aliases, nameof(IMember.IsApproved), IsApproved);
            EnsureMemberProperty(properties, aliases, nameof(IMember.IsLockedOut), IsLockedOut);
            EnsureMemberProperty(properties, aliases, nameof(IMember.LastLockoutDate), LastLockoutDate);
            EnsureMemberProperty(properties, aliases, nameof(IMember.CreateDate), CreateDate);
            EnsureMemberProperty(properties, aliases, nameof(IMember.LastLoginDate), LastLoginDate);
            EnsureMemberProperty(properties, aliases, nameof(IMember.LastPasswordChangeDate), LastPasswordChangeDate);
        }

        private void EnsureMemberProperty(List<IPublishedProperty> properties, List<string> aliases, string alias, object value)
        {
            // if the property already has a value, nothing to do
            if (aliases.Contains(alias)) return;

            // if not a property type, ignore
            var propertyType = ContentType.GetPropertyType(alias);
            if (propertyType == null) return;

            // create a raw-value property
            // note: throws if propertyType variations is not InvariantNeutral
            var property = new RawValueProperty(propertyType, this, value);
            properties.Add(property);
        }

        public override IPublishedContentType ContentType => _publishedMemberType;

        public override int Id => _member.Id;

        public override Guid Key => _member.Key;

        public override int? TemplateId => throw new NotSupportedException();

        public override int SortOrder => 0;

        public override string Name => _member.Name;

        public override IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => throw new NotSupportedException();

        public override string UrlSegment => throw new NotSupportedException();

        public override int WriterId => _member.CreatorId;

        public override int CreatorId => _member.CreatorId;

        public override string Path => _member.Path;

        public override DateTime CreateDate => _member.CreateDate;

        public override DateTime UpdateDate => _member.UpdateDate;

        public override int Level => _member.Level;

        public DateTime LastPasswordChangedDate => throw new NotImplementedException();

        #endregion
    }
}
