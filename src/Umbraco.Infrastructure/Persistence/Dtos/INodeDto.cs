using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

public interface INodeDto
{
    internal const string NodeIdColumnName = "nodeId";

    [Column(NodeIdColumnName)]
    int NodeId { get; }
}
