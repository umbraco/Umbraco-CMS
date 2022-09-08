using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

// note
// the whole PublishedMember thing should be refactored because as soon as a member
// is wrapped on in a model, the inner IMember and all associated properties are lost
internal class PublishedMember : PublishedContent
{
    private PublishedMember(
        IMember member,
        ContentNode contentNode,
        ContentData contentData,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IVariationContextAccessor variationContextAccessor,
        IPublishedModelFactory publishedModelFactory)
        : base(contentNode, contentData, publishedSnapshotAccessor, variationContextAccessor, publishedModelFactory) =>
        Member = member;

    public static IPublishedContent? Create(
        IMember member,
        IPublishedContentType contentType,
        bool previewing,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IVariationContextAccessor variationContextAccessor,
        IPublishedModelFactory publishedModelFactory)
    {
        var d = new ContentData(member.Name, null, 0, member.UpdateDate, member.CreatorId, -1, previewing, GetPropertyValues(contentType, member), null);

        var n = new ContentNode(
            member.Id,
            member.Key,
            contentType,
            member.Level,
            member.Path,
            member.SortOrder,
            member.ParentId,
            member.CreateDate,
            member.CreatorId);

        return new PublishedMember(member, n, d, publishedSnapshotAccessor, variationContextAccessor, publishedModelFactory)
            .CreateModel(publishedModelFactory);
    }

    private static Dictionary<string, PropertyData[]> GetPropertyValues(IPublishedContentType contentType, IMember member)
    {
        // see node in PublishedSnapshotService
        // we do not (want to) support ConvertDbToXml/String

        // var propertyEditorResolver = PropertyEditorResolver.Current;

        // see note in MemberType.Variations
        // we don't want to support variations on members
        var properties = member
            .Properties

            // .Select(property =>
            // {
            //    var e = propertyEditorResolver.GetByAlias(property.PropertyType.PropertyEditorAlias);
            //    var v = e == null
            //        ? property.Value
            //        : e.ValueEditor.ConvertDbToString(property, property.PropertyType, ApplicationContext.Current.Services.DataTypeService);
            //    return new KeyValuePair<string, object>(property.Alias, v);
            // })
            // .ToDictionary(x => x.Key, x => x.Value);
            .ToDictionary(
                x => x.Alias,
                x => new[] { new PropertyData { Value = x.GetValue(), Culture = string.Empty, Segment = string.Empty } },
                StringComparer.OrdinalIgnoreCase);

        // see also PublishedContentType
        AddIf(contentType, properties, nameof(IMember.Email), member.Email);
        AddIf(contentType, properties, nameof(IMember.Username), member.Username);
        AddIf(contentType, properties, nameof(IMember.Comments), member.Comments);
        AddIf(contentType, properties, nameof(IMember.IsApproved), member.IsApproved);
        AddIf(contentType, properties, nameof(IMember.IsLockedOut), member.IsLockedOut);
        AddIf(contentType, properties, nameof(IMember.LastLockoutDate), member.LastLockoutDate);
        AddIf(contentType, properties, nameof(IMember.CreateDate), member.CreateDate);
        AddIf(contentType, properties, nameof(IMember.LastLoginDate), member.LastLoginDate);
        AddIf(contentType, properties, nameof(IMember.LastPasswordChangeDate), member.LastPasswordChangeDate);

        return properties;
    }

    private static void AddIf(IPublishedContentType contentType, IDictionary<string, PropertyData[]> properties, string alias, object? value)
    {
        IPublishedPropertyType? propertyType = contentType.GetPropertyType(alias);
        if (propertyType == null || propertyType.IsUserProperty)
        {
            return;
        }

        properties[alias] = new[] { new PropertyData { Value = value, Culture = string.Empty, Segment = string.Empty } };
    }

    #region IPublishedMember

    public IMember Member { get; }

    public string Email => Member.Email;

    public string UserName => Member.Username;

    public string? Comments => Member.Comments;

    public bool IsApproved => Member.IsApproved;

    public bool IsLockedOut => Member.IsLockedOut;

    public DateTime? LastLockoutDate => Member.LastLockoutDate;

    public DateTime CreationDate => Member.CreateDate;

    public DateTime? LastLoginDate => Member.LastLoginDate;

    public DateTime? LastPasswordChangedDate => Member.LastPasswordChangeDate;

    #endregion
}
