using System;
using System.Linq;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberReadOnlyFactory : IEntityFactory<IMember, MemberReadOnlyDto>
    {
        public IMember BuildEntity(MemberReadOnlyDto dto)
        {
            var member = new Member
                         {
                             Id = dto.NodeId,
                             CreateDate = dto.CreateDate,
                             UpdateDate = dto.UpdateDate,
                             Name = dto.Text,
                             Email = dto.Email,
                             Username = dto.LoginName,
                             Password = dto.Password,
                             ProviderUserKey = dto.UniqueId,
                             Trashed = dto.Trashed,
                             Key = dto.UniqueId.Value,
                             ProfileId = dto.UniqueId.Value,
                             ContentTypeId = dto.ContentTypeId,
                             ContentTypeAlias = dto.ContentTypeAlias
                         };

            ((IUmbracoEntity)member).CreatorId = dto.UserId.Value;
            ((IUmbracoEntity)member).Level = dto.Level;
            ((IUmbracoEntity)member).ParentId = dto.ParentId;
            ((IUmbracoEntity)member).Path = dto.Path;
            ((IUmbracoEntity)member).SortOrder = dto.SortOrder;

            var propertyTypes = GetStandardPropertyTypeStubs();
            var propertiesDictionary = dto.Properties.ToDictionary(x => x.Alias);
            foreach (var property in propertiesDictionary)
            {
                if (propertyTypes.ContainsKey(property.Key))
                {
                    UpdatePropertyType(propertyTypes[property.Key], property.Value);
                }
                else
                {
                    propertyTypes.Add(property.Key, CreateProperty(property.Value));
                }
            }
            
            var properties = CreateProperties(propertyTypes, propertiesDictionary);
            member.Properties = new PropertyCollection(properties);

            member.SetProviderUserKeyType(typeof(Guid));
            member.ResetDirtyProperties(false);
            return member;
        }

        public MemberReadOnlyDto BuildDto(IMember entity)
        {
            throw new System.NotImplementedException();
        }

        private IEnumerable<Property> CreateProperties(Dictionary<string, PropertyType> propertyTypes, Dictionary<string, PropertyDataReadOnlyDto> propertiesDictionary)
        {
            var properties = new List<Property>();
            foreach (var propertyType in propertyTypes)
            {
                if (propertiesDictionary.ContainsKey(propertyType.Key))
                {
                    var prop = propertiesDictionary[propertyType.Key];
                    if (prop.PropertyDataId.HasValue && prop.PropertyDataId.Value != default(int))
                    {
                        properties.Add(propertyType.Value.CreatePropertyFromRawValue(prop.GetValue, prop.VersionId, prop.PropertyDataId.Value));
                    }
                    else
                    {
                        properties.Add(propertyType.Value.CreatePropertyFromValue(prop.GetValue));
                    }
                }
                else
                {
                    properties.Add(propertyType.Value.CreatePropertyFromValue(null));
                }
            }

            return properties;
        }

        private PropertyType CreateProperty(PropertyDataReadOnlyDto property)
        {
            var propertyType = new PropertyType(property.ControlId, property.DbType.EnumParse<DataTypeDatabaseType>(true))
                               {
                                   Id = property.Id,
                                   Alias = property.Alias,
                                   Name = property.Name,
                                   Description = property.Description,
                                   HelpText = property.HelpText,
                                   Mandatory = property.Mandatory,
                                   ValidationRegExp = property.ValidationRegExp,
                                   SortOrder = property.SortOrder
                               };

            if(property.PropertyTypeGroupId.HasValue)
                propertyType.PropertyGroupId = new Lazy<int>(() => property.PropertyTypeGroupId.Value);

            return propertyType;
        }

        private void UpdatePropertyType(PropertyType propertyType, PropertyDataReadOnlyDto property)
        {
            propertyType.Id = property.Id;
            propertyType.Alias = property.Alias;
            propertyType.Name = property.Name;
            propertyType.Description = property.Description;
            propertyType.HelpText = property.HelpText;
            propertyType.Mandatory = property.Mandatory;
            propertyType.ValidationRegExp = property.ValidationRegExp;
            propertyType.SortOrder = property.SortOrder;

            if (property.PropertyTypeGroupId.HasValue)
                propertyType.PropertyGroupId = new Lazy<int>(() => property.PropertyTypeGroupId.Value);
        }

        private Dictionary<string, PropertyType> GetStandardPropertyTypeStubs()
        {
            var propertyTypes = new Dictionary<string, PropertyType>();

            propertyTypes.Add(Constants.Conventions.Member.Comments,
                new PropertyType(new Guid(Constants.PropertyEditors.TextboxMultiple), DataTypeDatabaseType.Ntext)
                {
                    Alias = Constants.Conventions.Member.Comments,
                    Name = Constants.Conventions.Member.CommentsLabel
                });

            propertyTypes.Add(Constants.Conventions.Member.FailedPasswordAttempts,
                new PropertyType(new Guid(Constants.PropertyEditors.Integer), DataTypeDatabaseType.Integer)
                {
                    Alias = Constants.Conventions.Member.FailedPasswordAttempts,
                    Name = Constants.Conventions.Member.FailedPasswordAttemptsLabel
                });

            propertyTypes.Add(Constants.Conventions.Member.IsApproved,
                new PropertyType(new Guid(Constants.PropertyEditors.TrueFalse), DataTypeDatabaseType.Integer)
                {
                    Alias = Constants.Conventions.Member.IsApproved,
                    Name = Constants.Conventions.Member.IsApprovedLabel
                });

            propertyTypes.Add(Constants.Conventions.Member.IsLockedOut,
                new PropertyType(new Guid(Constants.PropertyEditors.TrueFalse), DataTypeDatabaseType.Integer)
                {
                    Alias = Constants.Conventions.Member.IsLockedOut,
                    Name = Constants.Conventions.Member.IsLockedOutLabel
                });

            propertyTypes.Add(Constants.Conventions.Member.LastLockoutDate,
                new PropertyType(new Guid(Constants.PropertyEditors.Date), DataTypeDatabaseType.Date)
                {
                    Alias = Constants.Conventions.Member.LastLockoutDate,
                    Name = Constants.Conventions.Member.LastLockoutDateLabel
                });

            propertyTypes.Add(Constants.Conventions.Member.LastLoginDate,
                new PropertyType(new Guid(Constants.PropertyEditors.Date), DataTypeDatabaseType.Date)
                {
                    Alias = Constants.Conventions.Member.LastLoginDate,
                    Name = Constants.Conventions.Member.LastLoginDateLabel
                });

            propertyTypes.Add(Constants.Conventions.Member.LastPasswordChangeDate,
                new PropertyType(new Guid(Constants.PropertyEditors.Date), DataTypeDatabaseType.Date)
                {
                    Alias = Constants.Conventions.Member.LastPasswordChangeDate,
                    Name = Constants.Conventions.Member.LastPasswordChangeDateLabel
                });

            propertyTypes.Add(Constants.Conventions.Member.PasswordAnswer,
                new PropertyType(new Guid(Constants.PropertyEditors.Textbox), DataTypeDatabaseType.Nvarchar)
                {
                    Alias = Constants.Conventions.Member.PasswordAnswer,
                    Name = Constants.Conventions.Member.PasswordAnswerLabel
                });

            propertyTypes.Add(Constants.Conventions.Member.PasswordQuestion,
                new PropertyType(new Guid(Constants.PropertyEditors.Textbox), DataTypeDatabaseType.Nvarchar)
                {
                    Alias = Constants.Conventions.Member.PasswordQuestion,
                    Name = Constants.Conventions.Member.PasswordQuestionLabel
                });

            return propertyTypes;
        }
    }
}