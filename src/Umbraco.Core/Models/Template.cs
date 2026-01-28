using System.Runtime.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Template file.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Template : File, ITemplate
{
    private readonly IShortStringHelper _shortStringHelper;
    private string _alias;
    private string? _layoutAlias;
    private Lazy<int>? _layoutId;
    private string? _name;

    public Template(IShortStringHelper shortStringHelper, string? name, string? alias)
        : this(shortStringHelper, name, alias, null)
    {
    }

    public Template(IShortStringHelper shortStringHelper, string? name, string? alias, Func<File, string?>? getFileContent)
        : base(string.Empty, getFileContent)
    {
        _shortStringHelper = shortStringHelper;
        _name = name;
        _alias = alias?.ToCleanString(shortStringHelper, CleanStringType.UnderscoreAlias) ?? string.Empty;
        _layoutId = new Lazy<int>(() => -1);
    }

    [DataMember]
    public Lazy<int>? LayoutId
    {
        get => _layoutId;
        set => SetPropertyValueAndDetectChanges(value, ref _layoutId, nameof(LayoutId));
    }

    public string? LayoutAlias
    {
        get => _layoutAlias;
        set => SetPropertyValueAndDetectChanges(value, ref _layoutAlias, nameof(LayoutAlias));
    }

    [DataMember]
    public new string? Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
    }

    [DataMember]
    public new string Alias
    {
        get => _alias;
        set => SetPropertyValueAndDetectChanges(
            value.ToCleanString(_shortStringHelper, CleanStringType.UnderscoreAlias), ref _alias!, nameof(Alias));
    }

    /// <summary>
    ///     Returns true if the template is used as a layout for other templates (i.e. it has 'children')
    /// </summary>
    public bool IsLayout { get; set; }

    /// <summary>
    ///     Returns the master template alias (the parent template this template inherits from)
    /// </summary>
    [Obsolete("Use LayoutAlias instead. This will be removed in Umbraco 19.")]
    public string? MasterTemplateAlias { get => LayoutAlias; set => LayoutAlias = value; }

    /// <summary>
    ///     Returns the Id of the master template
    /// </summary>
    [Obsolete("Use LayoutId instead. This will be removed in Umbraco 19.")]
    public Lazy<int>? MasterTemplateId { get => LayoutId; set => LayoutId = value; }

    // FIXME: moving forward the layout is calculated from the actual template content; figure out how to get rid of this method, or at least *only* use it from TemplateService
    [Obsolete("Layout is now calculated from the content. This will be removed in Umbraco 19.")]
    public void SetLayout(ITemplate? layout)
    {
        if (layout == null)
        {
            LayoutId = new Lazy<int>(() => -1);
            LayoutAlias = null;
        }
        else
        {
            LayoutId = new Lazy<int>(() => layout.Id);
            LayoutAlias = layout.Alias;
        }
    }

    protected override void DeepCloneNameAndAlias(File clone)
    {
        // do nothing - prevents File from doing its stuff
    }
}
