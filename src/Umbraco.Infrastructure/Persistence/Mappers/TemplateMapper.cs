using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="Template" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(Template))]
[MapperFor(typeof(ITemplate))]
public sealed class TemplateMapper : BaseMapper
{
    public TemplateMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<Template, TemplateDto>(nameof(Template.Id), nameof(TemplateDto.NodeId));
        DefineMap<Template, NodeDto>(nameof(Template.MasterTemplateId), nameof(NodeDto.ParentId));
        DefineMap<Template, NodeDto>(nameof(Template.Key), nameof(NodeDto.UniqueId));
        DefineMap<Template, TemplateDto>(nameof(Template.Alias), nameof(TemplateDto.Alias));
    }
}
