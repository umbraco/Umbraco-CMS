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
        _masterTemplateId = new Lazy<int>(() => -1);
    }

    [DataMember]
    public Lazy<int>? MasterTemplateId
    {
        get => _masterTemplateId;
        set => SetPropertyValueAndDetectChanges(value, ref _masterTemplateId, nameof(MasterTemplateId));
    }

    public string? MasterTemplateAlias
    {
        get => _masterTemplateAlias;
        set => SetPropertyValueAndDetectChanges(value, ref _masterTemplateAlias, nameof(MasterTemplateAlias));
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
    public bool IsMasterTemplate { get; set; }

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

    protected override void DeepCloneNameAndAlias(File clone)
    {
        // do nothing - prevents File from doing its stuff
    }
}
