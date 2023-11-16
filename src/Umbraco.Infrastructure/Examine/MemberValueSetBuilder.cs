using Examine;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

public class MemberValueSetBuilder : BaseValueSetBuilder<IMember>
{
    private readonly IContentTypeService _contentTypeService;

    public MemberValueSetBuilder(PropertyEditorCollection propertyEditors, IContentTypeService contentTypeService)
        : base(propertyEditors, false)
    {
        _contentTypeService = contentTypeService;
    }

    [Obsolete("Use non-obsolete ctor, scheduled for removal in v14")]
    public MemberValueSetBuilder(PropertyEditorCollection propertyEditors)
        : this(propertyEditors, StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>())
    {
    }

    /// <inheritdoc />
    public override IEnumerable<ValueSet> GetValueSets(params IMember[] members)
    {
        IDictionary<Guid, IContentType> contentTypeDictionary = _contentTypeService.GetAll().ToDictionary(x => x.Key);

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

            foreach (IProperty property in m.Properties)
            {
                AddPropertyValue(property, null, null, values, m.AvailableCultures, contentTypeDictionary);
            }

            var vs = new ValueSet(m.Id.ToInvariantString(), IndexTypes.Member, m.ContentType.Alias, values);

            yield return vs;
        }
    }
}
