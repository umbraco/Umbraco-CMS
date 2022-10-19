using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface ILogViewerQuery : IEntity
{
    string? Name { get; set; }

    string? Query { get; set; }
}
