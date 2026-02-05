using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

internal interface INodeDto
{
    internal static class Columns
    {
        internal const string NodeId = Constants.DatabaseSchema.Columns.NodeIdName;
    }

    [Column(Columns.NodeId)]
    int NodeId { get; set; }
}
