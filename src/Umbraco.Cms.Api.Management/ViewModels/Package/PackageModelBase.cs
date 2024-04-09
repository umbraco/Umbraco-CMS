namespace Umbraco.Cms.Api.Management.ViewModels.Package;

public class PackageModelBase
{
    /// <summary>
    ///     Gets or sets the name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the id of the selected content node.
    /// </summary>
    public string? ContentNodeId { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to load all child nodes of the selected content node.
    /// </summary>
    public bool ContentLoadChildNodes { get; set; }

    /// <summary>
    ///     Gets or sets the list of media keys for the selected media items.
    /// </summary>
    public IList<Guid> MediaIds { get; set; } = new List<Guid>();

    /// <summary>
    ///     Gets or sets a value indicating whether to load all child nodes of the selected media items.
    /// </summary>
    public bool MediaLoadChildNodes { get; set; }

    /// <summary>
    ///     Gets or sets the list of ids for the selected document types.
    /// </summary>
    public IList<string> DocumentTypes { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the list of ids for the selected media types.
    /// </summary>
    public IList<string> MediaTypes { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the list of ids for the selected data types.
    /// </summary>
    public IList<string> DataTypes { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the list of ids for the selected templates.
    /// </summary>
    public IList<string> Templates { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the list of relative paths for the selected partial views.
    /// </summary>
    public IList<string> PartialViews { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the list of names for the selected stylesheets.
    /// </summary>
    public IList<string> Stylesheets { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the list of names for the selected scripts.
    /// </summary>
    public IList<string> Scripts { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the list of ids for the selected languages.
    /// </summary>
    public IList<string> Languages { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the list of ids for the selected dictionary items.
    /// </summary>
    public IList<string> DictionaryItems { get; set; } = new List<string>();
}
