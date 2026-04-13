using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="Element" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(Element))]
[MapperFor(typeof(IElement))]
public sealed class ElementMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">The lazily-initialized SQL context used for building SQL expressions.</param>
    /// <param name="maps">The mapper configuration store that holds the property-to-column mappings.</param>
    public ElementMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    /// <inheritdoc />
    protected override void DefineMaps()
    {
        DefineMap<Element, NodeDto>(nameof(Element.Id), nameof(NodeDto.NodeId));
        DefineMap<Element, NodeDto>(nameof(Element.Key), nameof(NodeDto.UniqueId));

        DefineMap<Element, ContentVersionDto>(nameof(Element.VersionId), nameof(ContentVersionDto.Id));
        DefineMap<Element, ContentVersionDto>(nameof(Element.Name), nameof(ContentVersionDto.Text));

        DefineMap<Element, NodeDto>(nameof(Element.ParentId), nameof(NodeDto.ParentId));
        DefineMap<Element, NodeDto>(nameof(Element.Level), nameof(NodeDto.Level));
        DefineMap<Element, NodeDto>(nameof(Element.Path), nameof(NodeDto.Path));
        DefineMap<Element, NodeDto>(nameof(Element.SortOrder), nameof(NodeDto.SortOrder));
        DefineMap<Element, NodeDto>(nameof(Element.Trashed), nameof(NodeDto.Trashed));

        DefineMap<Element, NodeDto>(nameof(Element.CreateDate), nameof(NodeDto.CreateDate));
        DefineMap<Element, NodeDto>(nameof(Element.CreatorId), nameof(NodeDto.UserId));
        DefineMap<Element, ContentDto>(nameof(Element.ContentTypeId), nameof(ContentDto.ContentTypeId));

        DefineMap<Element, ContentVersionDto>(nameof(Element.UpdateDate), nameof(ContentVersionDto.VersionDate));
        DefineMap<Element, ElementDto>(nameof(Element.Published), nameof(ElementDto.Published));
    }
}
