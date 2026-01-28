using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

public interface INodeDto
{
    internal const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    [Column(NodeIdColumnName)]
    int NodeId { get; }
}
