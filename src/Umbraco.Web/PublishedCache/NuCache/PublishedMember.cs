using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // note
    // the whole PublishedMember thing should be refactored because as soon as a member
    // is wrapped on in a model, the inner IMember and all associated properties are lost

    internal class PublishedMember : PublishedContent //, IPublishedMember
    {
        private readonly IMember _member;

        private PublishedMember(IMember member, ContentNode contentNode, ContentData contentData, IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor)
            : base(contentNode, contentData, publishedSnapshotAccessor, variationContextAccessor)
        {
            _member = member;
        }

        public static IPublishedContent Create(IMember member, PublishedContentType contentType, bool previewing, IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor)
        {
            var d = new ContentData
            {
                Name = member.Name,
                Published = previewing,
                TemplateId = -1,
                VersionDate = member.UpdateDate,
                WriterId = member.CreatorId, // what else?
                Properties = GetPropertyValues(contentType, member)
            };
            var n = new ContentNode(member.Id, member.Key,
                contentType,
                member.Level, member.Path, member.SortOrder,
                member.ParentId,
                member.CreateDate, member.CreatorId);
            return new PublishedMember(member, n, d, publishedSnapshotAccessor, variationContextAccessor).CreateModel();
        }

        private static Dictionary<string, PropertyData[]> GetPropertyValues(PublishedContentType contentType, IMember member)
        {
            // see node in PublishedSnapshotService
            // we do not (want to) support ConvertDbToXml/String

            //var propertyEditorResolver = PropertyEditorResolver.Current;

            // see note in MemberType.Variations
            // we don't want to support variations on members

            var properties = member
                .Properties
                //.Select(property =>
                //{
                //    var e = propertyEditorResolver.GetByAlias(property.PropertyType.PropertyEditorAlias);
                //    var v = e == null
                //        ? property.Value
                //        : e.ValueEditor.ConvertDbToString(property, property.PropertyType, ApplicationContext.Current.Services.DataTypeService);
                //    return new KeyValuePair<string, object>(property.Alias, v);
                //})
                //.ToDictionary(x => x.Key, x => x.Value);
                .ToDictionary(x => x.Alias, x => new[] { new PropertyData { Value = x.GetValue() } }, StringComparer.OrdinalIgnoreCase);

            // see also PublishedContentType
            AddIf(contentType, properties, "Email", member.Email);
            AddIf(contentType, properties, "Username", member.Username);
            AddIf(contentType, properties, "PasswordQuestion", member.PasswordQuestion);
            AddIf(contentType, properties, "Comments", member.Comments);
            AddIf(contentType, properties, "IsApproved", member.IsApproved);
            AddIf(contentType, properties, "IsLockedOut", member.IsLockedOut);
            AddIf(contentType, properties, "LastLockoutDate", member.LastLockoutDate);
            AddIf(contentType, properties, "CreateDate", member.CreateDate);
            AddIf(contentType, properties, "LastLoginDate", member.LastLoginDate);
            AddIf(contentType, properties, "LastPasswordChangeDate", member.LastPasswordChangeDate);

            return properties;
        }

        private static void AddIf(PublishedContentType contentType, IDictionary<string, PropertyData[]> properties, string alias, object value)
        {
            var propertyType = contentType.GetPropertyType(alias);
            if (propertyType == null || propertyType.IsUserProperty) return;
            properties[alias] = new[] { new PropertyData { Value = value } };
        }

        #region IPublishedMember

        public IMember Member => _member;

        public string Email => _member.Email;

        public string UserName => _member.Username;

        public string PasswordQuestion => _member.PasswordQuestion;

        public string Comments => _member.Comments;

        public bool IsApproved => _member.IsApproved;

        public bool IsLockedOut => _member.IsLockedOut;

        public DateTime LastLockoutDate => _member.LastLockoutDate;

        public DateTime CreationDate => _member.CreateDate;

        public DateTime LastLoginDate => _member.LastLoginDate;

        public DateTime LastActivityDate => _member.LastLoginDate;

        public DateTime LastPasswordChangedDate => _member.LastPasswordChangeDate;

        #endregion
    }
}
