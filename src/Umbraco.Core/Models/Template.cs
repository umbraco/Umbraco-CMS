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
    private string? _masterTemplateAlias;
    private Lazy<int>? _masterTemplateId;
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
        _masterTemplateId = new Lazy<int>(() => -1);
    }

    /// <summary>
    ///     Gets or sets the master template identifier as a lazy-loaded value.
    /// </summary>
    /// <value>
    ///     A <see cref="Lazy{T}" /> containing the master template's ID, or -1 if there is no master template.
    /// </value>
    [DataMember]
    public Lazy<int>? MasterTemplateId
    {
        get => _masterTemplateId;
        set => SetPropertyValueAndDetectChanges(value, ref _masterTemplateId, nameof(MasterTemplateId));
    }

    /// <summary>
    ///     Gets or sets the alias of the master (parent) template.
    /// </summary>
    /// <value>
    ///     The alias of the master template, or <c>null</c> if this template has no master.
    /// </value>
    public string? MasterTemplateAlias
    {
        get => _masterTemplateAlias;
        set => SetPropertyValueAndDetectChanges(value, ref _masterTemplateAlias, nameof(MasterTemplateAlias));
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
    public bool IsMasterTemplate { get; set; }

    // FIXME: moving forward the master template is calculated from the actual template content; figure out how to get rid of this method, or at least *only* use it from TemplateService
    /// <summary>
    ///     Sets the master template for this template.
    /// </summary>
    /// <param name="masterTemplate">The master template to set, or <c>null</c> to remove the master template.</param>
    [Obsolete("MasterTemplate is now calculated from the content. This will be removed in Umbraco 15.")]
    public void SetMasterTemplate(ITemplate? masterTemplate)
    {
        if (masterTemplate == null)
        {
            MasterTemplateId = new Lazy<int>(() => -1);
            MasterTemplateAlias = null;
        }
        else
        {
            MasterTemplateId = new Lazy<int>(() => masterTemplate.Id);
            MasterTemplateAlias = masterTemplate.Alias;
        }
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
