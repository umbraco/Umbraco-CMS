using Examine;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Provides functionality to build value sets for member entities, enabling them to be indexed by Examine.
/// </summary>
/// <remarks>
/// Values of properties marked as sensitive on the member type are not included, as an index has no means of
/// restricting them to users with access to sensitive data.
/// </remarks>
public class MemberValueSetBuilder : BaseValueSetBuilder<IMember>
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IMemberTypeService _memberTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberValueSetBuilder"/> class with the specified property editors and content type service.
    /// </summary>
    /// <param name="propertyEditors">A <see cref="PropertyEditorCollection"/> representing the collection of property editors to use.</param>
    /// <param name="contentTypeService">An <see cref="IContentTypeService"/> used to access content type information.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 20.")]
    public MemberValueSetBuilder(PropertyEditorCollection propertyEditors, IContentTypeService contentTypeService)
        : this(
            propertyEditors,
            contentTypeService,
            StaticServiceProvider.Instance.GetRequiredService<IMemberTypeService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberValueSetBuilder"/> class.
    /// </summary>
    /// <param name="propertyEditors">A <see cref="PropertyEditorCollection"/> representing the collection of property editors to use.</param>
    /// <param name="contentTypeService">An <see cref="IContentTypeService"/> used to access content type information.</param>
    /// <param name="memberTypeService">An <see cref="IMemberTypeService"/> used to determine which member properties are sensitive.</param>
    public MemberValueSetBuilder(PropertyEditorCollection propertyEditors, IContentTypeService contentTypeService, IMemberTypeService memberTypeService)
        : base(propertyEditors, false)
    {
        _contentTypeService = contentTypeService;
        _memberTypeService = memberTypeService;
    }

    /// <inheritdoc />
    public override IEnumerable<ValueSet> GetValueSets(params IMember[] members)
    {
        IDictionary<Guid, IContentType> contentTypeDictionary = _contentTypeService.GetAll().ToDictionary(x => x.Key);
        Dictionary<Guid, IMemberType> memberTypes = _memberTypeService
            .GetMany(members.Select(m => m.ContentType.Key).Distinct())
            .ToDictionary(x => x.Key);

        foreach (IMember m in members)
        {
            var values = new Dictionary<string, IEnumerable<object?>>
            {
                { "icon", m.ContentType.Icon?.Yield() ?? Enumerable.Empty<string>() },
                { "id", new object[] { m.Id } },
                { UmbracoExamineFieldNames.NodeKeyFieldName, new object[] { m.Key } },
                { "parentID", new object[] { m.Level > 1 ? m.ParentId : -1 } },
                { "level", new object[] { m.Level } },
                { "creatorID", new object[] { m.CreatorId } },
                { "sortOrder", new object[] { m.SortOrder } },
                { "createDate", new object[] { m.CreateDate } },
                { "updateDate", new object[] { m.UpdateDate } },
                { UmbracoExamineFieldNames.NodeNameFieldName, m.Name?.Yield() ?? Enumerable.Empty<string>() },
                { "path", m.Path.Yield() },
                { "nodeType", m.ContentType.Id.ToString().Yield() },
                { "loginName", m.Username.Yield() },
                { "email", m.Email.Yield() },
            };

            memberTypes.TryGetValue(m.ContentType.Key, out IMemberType? memberType);
            foreach (IProperty property in m.Properties)
            {
                // Without the member type, sensitive properties can't be identified, so none are indexed.
                if (memberType is null || memberType.IsSensitiveProperty(property.Alias))
                {
                    continue;
                }

                AddPropertyValue(property, null, null, values, m.AvailableCultures, contentTypeDictionary);
            }

            var vs = new ValueSet(m.Id.ToInvariantString(), IndexTypes.Member, m.ContentType.Alias, values);

            yield return vs;
        }
    }
}
