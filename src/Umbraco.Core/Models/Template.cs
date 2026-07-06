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
    private string? _layoutTemplateAlias;
    private Lazy<int>? _layoutTemplateId;
    private string? _name;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Template" /> class.
    /// </summary>
    /// <param name="shortStringHelper">The short string helper for alias cleaning.</param>
    /// <param name="name">The name of the template.</param>
    /// <param name="alias">The alias of the template.</param>
    public Template(IShortStringHelper shortStringHelper, string? name, string? alias)
        : this(shortStringHelper, name, alias, null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Template" /> class.
    /// </summary>
    /// <param name="shortStringHelper">The short string helper for alias cleaning.</param>
    /// <param name="name">The name of the template.</param>
    /// <param name="alias">The alias of the template.</param>
    /// <param name="getFileContent">A function to retrieve the file content lazily.</param>
    public Template(IShortStringHelper shortStringHelper, string? name, string? alias, Func<File, string?>? getFileContent)
        : base(string.Empty, getFileContent)
    {
        _shortStringHelper = shortStringHelper;
        _name = name;
        _alias = alias?.ToCleanString(shortStringHelper, CleanStringType.UnderscoreAlias) ?? string.Empty;
        _layoutTemplateId = new Lazy<int>(() => -1);
    }

    /// <summary>
    ///     Gets or sets the layout template identifier as a lazy-loaded value.
    /// </summary>
    /// <value>
    ///     A <see cref="Lazy{T}" /> containing the layout template's ID, or -1 if there is no layout template.
    /// </value>
    [DataMember]
    public Lazy<int>? LayoutTemplateId
    {
        get => _layoutTemplateId;
        set => SetPropertyValueAndDetectChanges(value, ref _layoutTemplateId, nameof(LayoutTemplateId));
    }

    /// <summary>
    ///     Gets or sets the alias of the layout (parent) template.
    /// </summary>
    /// <value>
    ///     The alias of the layout template, or <c>null</c> if this template has no layout.
    /// </value>
    public string? LayoutTemplateAlias
    {
        get => _layoutTemplateAlias;
        set => SetPropertyValueAndDetectChanges(value, ref _layoutTemplateAlias, nameof(LayoutTemplateAlias));
    }

    /// <summary>
    ///     Gets or sets the name of the template.
    /// </summary>
    [DataMember]
    public new string? Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
    }

    /// <summary>
    ///     Gets or sets the alias of the template.
    /// </summary>
    /// <remarks>
    ///     The alias is automatically cleaned using the underscore alias format.
    /// </remarks>
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
    public bool IsLayoutTemplate { get; set; }

    /// <inheritdoc cref="LayoutTemplateId" />
    [Obsolete("Use LayoutTemplateId instead. Scheduled for removal in Umbraco 20.")]
    public Lazy<int>? MasterTemplateId
    {
        get => LayoutTemplateId;
        set => LayoutTemplateId = value;
    }

    /// <inheritdoc cref="LayoutTemplateAlias" />
    [Obsolete("Use LayoutTemplateAlias instead. Scheduled for removal in Umbraco 20.")]
    public new string? MasterTemplateAlias
    {
        get => LayoutTemplateAlias;
        set => LayoutTemplateAlias = value;
    }

    /// <inheritdoc cref="IsLayoutTemplate" />
    [Obsolete("Use IsLayoutTemplate instead. Scheduled for removal in Umbraco 20.")]
    public bool IsMasterTemplate
    {
        get => IsLayoutTemplate;
        set => IsLayoutTemplate = value;
    }

    /// <summary>
    ///     Overrides the base implementation to prevent File from cloning name and alias.
    /// </summary>
    /// <param name="clone">The cloned file instance.</param>
    protected override void DeepCloneNameAndAlias(File clone)
    {
        // do nothing - prevents File from doing its stuff
    }
}
