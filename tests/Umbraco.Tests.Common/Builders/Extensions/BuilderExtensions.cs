// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders.Extensions;

public static class BuilderExtensions
{
    public static T WithId<T>(this T builder, int id)
        where T : IWithIdBuilder
    {
        builder.Id = id;
        return builder;
    }

    public static T WithId<T, TId>(this T builder, TId id)
        where T : IWithIdBuilder<TId>
    {
        builder.Id = id;
        return builder;
    }

    public static T WithoutIdentity<T>(this T builder)
        where T : IWithIdBuilder
    {
        builder.Id = 0;
        return builder;
    }

    public static T WithCreatorId<T>(this T builder, int creatorId)
        where T : IWithCreatorIdBuilder
    {
        builder.CreatorId = creatorId;
        return builder;
    }

    public static T WithCreateDate<T>(this T builder, DateTime createDate)
        where T : IWithCreateDateBuilder
    {
        builder.CreateDate = createDate;
        return builder;
    }

    public static T WithUpdateDate<T>(this T builder, DateTime updateDate)
        where T : IWithUpdateDateBuilder
    {
        builder.UpdateDate = updateDate;
        return builder;
    }

    public static T WithDeleteDate<T>(this T builder, DateTime deleteDate)
        where T : IWithDeleteDateBuilder
    {
        builder.DeleteDate = deleteDate;
        return builder;
    }

    public static T WithAlias<T>(this T builder, string alias)
        where T : IWithAliasBuilder
    {
        builder.Alias = alias;
        return builder;
    }

    public static T WithName<T>(this T builder, string name)
        where T : IWithNameBuilder
    {
        builder.Name = name;
        return builder;
    }

    public static T WithKey<T>(this T builder, Guid key)
        where T : IWithKeyBuilder
    {
        builder.Key = key;
        return builder;
    }

    public static T WithDataTypeKey<T>(this T builder, Guid key)
        where T : IWithDataTypeKeyBuilder
    {
        builder.DataTypeKey = key;
        return builder;
    }

    public static T WithParentId<T>(this T builder, int parentId)
        where T : IWithParentIdBuilder
    {
        builder.ParentId = parentId;
        return builder;
    }

    public static T WithParentContentType<T>(this T builder, IContentTypeComposition parent)
        where T : IWithParentContentTypeBuilder
    {
        builder.Parent = parent;
        return builder;
    }

    public static T WithTrashed<T>(this T builder, bool trashed)
        where T : IWithTrashedBuilder
    {
        builder.Trashed = trashed;
        return builder;
    }

    public static T WithLevel<T>(this T builder, int level)
        where T : IWithLevelBuilder
    {
        builder.Level = level;
        return builder;
    }

    public static T WithPath<T>(this T builder, string path)
        where T : IWithPathBuilder
    {
        builder.Path = path;
        return builder;
    }

    public static T WithSortOrder<T>(this T builder, int sortOrder)
        where T : IWithSortOrderBuilder
    {
        builder.SortOrder = sortOrder;
        return builder;
    }

    public static T WithDescription<T>(this T builder, string description)
        where T : IWithDescriptionBuilder
    {
        builder.Description = description;
        return builder;
    }

    public static T WithIcon<T>(this T builder, string icon)
        where T : IWithIconBuilder
    {
        builder.Icon = icon;
        return builder;
    }

    public static T WithThumbnail<T>(this T builder, string thumbnail)
        where T : IWithThumbnailBuilder
    {
        builder.Thumbnail = thumbnail;
        return builder;
    }

    public static T WithLogin<T>(this T builder, string username, string rawPasswordValue)
        where T : IWithLoginBuilder
    {
        builder.Username = username;
        builder.RawPasswordValue = rawPasswordValue;
        return builder;
    }

    public static T WithEmail<T>(this T builder, string email)
        where T : IWithEmailBuilder
    {
        builder.Email = email;
        return builder;
    }

    public static T WithFailedPasswordAttempts<T>(this T builder, int failedPasswordAttempts)
        where T : IWithFailedPasswordAttemptsBuilder
    {
        builder.FailedPasswordAttempts = failedPasswordAttempts;
        return builder;
    }

    public static T WithIsApproved<T>(this T builder, bool isApproved)
        where T : IWithIsApprovedBuilder
    {
        builder.IsApproved = isApproved;
        return builder;
    }

    public static T WithIsLockedOut<T>(this T builder, bool isLockedOut, DateTime? lastLockoutDate = null)
        where T : IWithIsLockedOutBuilder
    {
        builder.IsLockedOut = isLockedOut;
        if (lastLockoutDate.HasValue)
        {
            builder.LastLockoutDate = lastLockoutDate.Value;
        }

        return builder;
    }

    public static T WithLastLoginDate<T>(this T builder, DateTime lastLoginDate)
        where T : IWithLastLoginDateBuilder
    {
        builder.LastLoginDate = lastLoginDate;
        return builder;
    }

    public static T WithLastPasswordChangeDate<T>(this T builder, DateTime lastPasswordChangeDate)
        where T : IWithLastPasswordChangeDateBuilder
    {
        builder.LastPasswordChangeDate = lastPasswordChangeDate;
        return builder;
    }

    public static T WithPropertyTypeIdsIncrementingFrom<T>(this T builder, int propertyTypeIdsIncrementingFrom)
        where T : IWithPropertyTypeIdsIncrementingFrom
    {
        builder.PropertyTypeIdsIncrementingFrom = propertyTypeIdsIncrementingFrom;
        return builder;
    }

    public static T WithIsContainer<T>(this T builder, Guid? listView)
        where T : IWithIsContainerBuilder
    {
        builder.ListView = listView;
        return builder;
    }

    public static T WithCultureInfo<T>(this T builder, string cultureCode)
        where T : IWithCultureInfoBuilder
    {
        builder.CultureInfo = CultureInfo.GetCultureInfo(cultureCode);
        return builder;
    }

    public static T WithSupportsPublishing<T>(this T builder, bool supportsPublishing)
        where T : IWithSupportsPublishing
    {
        builder.SupportsPublishing = supportsPublishing;
        return builder;
    }

    public static T WithPropertyValues<T>(this T builder, object propertyValues, string? culture = null, string? segment = null)
        where T : IWithPropertyValues
    {
        builder.PropertyValues = propertyValues;
        builder.PropertyValuesCulture = culture;
        builder.PropertyValuesSegment = segment;
        return builder;
    }

    public static T WithDate<T>(this T builder, DateTime date) where T : IWithDateBuilder
    {
        builder.Date = date;
        return builder;
    }

    public static T WithInvariantName<T>(this T builder, string invariantName)
        where T : IWithInvariantNameBuilder
    {
        builder.InvariantName = invariantName;
        return builder;
    }

    public static T WithKey<T>(this T builder, Guid? key)
        where T : IWithKeyBuilder
    {
        builder.Key = key;
        return builder;
    }

    public static T WithContentTypeKey<T>(this T builder, Guid contentTypeKey)
        where T : IWithContentTypeKeyBuilder
    {
        builder.ContentTypeKey = contentTypeKey;
        return builder;
    }

    public static T WithParentKey<T>(this T builder, Guid? parentKey)
        where T : IWithParentKeyBuilder
    {
        builder.ParentKey = parentKey;
        return builder;
    }

    public static T WithTemplateKey<T>(this T builder, Guid? templateKey)
        where T : IWithTemplateKeyBuilder
    {
        builder.TemplateKey = templateKey;
        return builder;
    }

    public static T WithValue<T>(this T builder, object? value)
        where T : IWithValueBuilder
    {
        builder.Value = value;
        return builder;
    }

    public static T WithCulture<T>(this T builder, string culture)
        where T : IWithCultureBuilder
    {
        builder.Culture = culture;
        return builder;
    }

    public static T WithAllowAsRoot<T>(this T builder, bool allowAsRoot)
        where T : IWithAllowAsRootBuilder
    {
        builder.AllowAsRoot = allowAsRoot;
        return builder;
    }

    public static T WithSegment<T>(this T builder, string segment)
        where T : IWithSegmentBuilder
    {
        builder.Segment = segment;
        return builder;
    }

    public static T WithIsElement<T>(this T builder, bool isElement)
        where T : IWithIsElementBuilder
    {
        builder.IsElement = isElement;
        return builder;
    }

    public static T WithVariesByCulture<T>(this T builder, bool variesByCulture)
        where T : IWithVariesByCultureBuilder
    {
        builder.VariesByCulture = variesByCulture;
        return builder;
    }

    public static T WithVariesBySegement<T>(this T builder, bool variesBySegment)
        where T : IWithVariesBySegmentBuilder
    {
        builder.VariesBySegment = variesBySegment;
        return builder;
    }

    public static T WithDefaultTemplateKey<T>(this T builder, Guid? defaultTemplateKey)
        where T : IWithDefaultTemplateKeyBuilder
    {
        builder.DefaultTemplateKey = defaultTemplateKey;
        return builder;
    }

    public static T WithContainerKey<T>(this T builder, Guid? containerKey)
        where T : IWithContainerKeyBuilder
    {
        builder.ContainerKey = containerKey;
        return builder;
    }
}
