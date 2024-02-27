namespace Umbraco.Cms.Core.Configuration.Models;

public class PropertySettings
{
    private readonly HashSet<string> _standardFieldNames = new HashSet<string>()
    {
        "createDate",
        "creatorName",
        "level",
        "nodeType",
        "nodeTypeAlias",
        "pageID",
        "pageName",
        "parentID",
        "path",
        "template",
        "updateDate",
        "writerID",
        "writerName"
    };

    /// <summary>
    /// Gets a set of standard names for fields that cannot be used for custom properties.
    /// </summary>
    public ISet<string> StandardFieldNames
    {
        get => _standardFieldNames;
    }

    public bool AddStandardFieldName(string name) => _standardFieldNames.Add(name);
}
