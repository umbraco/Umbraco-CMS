using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

public class IndexColumnDefinition
{
    public virtual string? Name { get; set; }

    public virtual Direction Direction { get; set; }
}
