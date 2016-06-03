using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // note
    // the whole PublishedMember thing should be refactored because as soon as a member
    // is wrapped on in a model, the inner IMember and all associated properties are lost

    class PublishedMember : PublishedContent //, IPublishedMember
    {
        private readonly IMember _member;

        private PublishedMember(IMember member, ContentNode contentNode, ContentData contentData, IFacadeAccessor facadeAccessor)
            : base(contentNode, contentData, facadeAccessor)
        {
            _member = member;
        }

        public static IPublishedContent Create(IMember member, PublishedContentType contentType, bool previewing, IFacadeAccessor facadeAccessor)
        {
            var d = new ContentData
            {
                Name = member.Name,
                Published = previewing,
                TemplateId = -1,
                Version = member.Version,
                VersionDate = member.UpdateDate,
                WriterId = member.CreatorId, // what else?
                Properties = GetPropertyValues(contentType, member)
            };
            var n = new ContentNode(member.Id, member.Key,
                contentType,
                member.Level, member.Path, member.SortOrder,
                member.ParentId,
                member.CreateDate, member.CreatorId);
            return new PublishedMember(member, n, d, facadeAccessor).CreateModel();
        }

        private static Dictionary<string, object> GetPropertyValues(PublishedContentType contentType, IMember member)
        {
            // see node in FacadeService
            // we do not (want to) support ConvertDbToXml/String

            //var propertyEditorResolver = PropertyEditorResolver.Current;

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
                .ToDictionary(x => x.Alias, x => x.Value, StringComparer.OrdinalIgnoreCase);

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

        private static void AddIf(PublishedContentType contentType, IDictionary<string, object> properties, string alias, object value)
        {
            var propertyType = contentType.GetPropertyType(alias);
            if (propertyType == null || propertyType.IsUmbraco == false) return;
            properties[alias] = value;
        }

        #region IPublishedMember

        public IMember Member
        {
            get { return _member; }
        }

        public string Email
        {
            get { return _member.Email; }
        }

        public string UserName
        {
            get { return _member.Username; }
        }

        public string PasswordQuestion
        {
            get { return _member.PasswordQuestion; }
        }

        public string Comments
        {
            get { return _member.Comments; }
        }

        public bool IsApproved
        {
            get { return _member.IsApproved; }
        }

        public bool IsLockedOut
        {
            get { return _member.IsLockedOut; }
        }

        public DateTime LastLockoutDate
        {
            get { return _member.LastLockoutDate; }
        }

        public DateTime CreationDate
        {
            get { return _member.CreateDate; }
        }

        public DateTime LastLoginDate
        {
            get { return _member.LastLoginDate; }
        }

        public DateTime LastActivityDate
        {
            get { return _member.LastLoginDate; }
        }

        public DateTime LastPasswordChangedDate
        {
            get { return _member.LastPasswordChangeDate; }
        }

        #endregion
    }
}
