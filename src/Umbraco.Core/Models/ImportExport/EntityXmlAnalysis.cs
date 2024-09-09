using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models.ImportExport;

public class EntityXmlAnalysis
{
    public UmbracoEntityTypes EntityType { get; set; }

    public string? Alias { get; set; }

    public Guid? Key { get; set; }
}
