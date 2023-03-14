using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Stylesheet Property
/// </summary>
/// <remarks>
///     Properties are always formatted to have a single selector, so it can be used in the backoffice
/// </remarks>
[Serializable]
[DataContract(IsReference = true)]
public class StylesheetProperty : BeingDirtyBase, IValueObject, IStylesheetProperty
{
    private string _alias;
    private string _value;

    public StylesheetProperty(string name, string alias, string value)
    {
        Name = name;
        _alias = alias;
        _value = value;
    }

    /// <summary>
    ///     The CSS rule name that can be used by Umbraco in the back office
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     This is the CSS Selector
    /// </summary>
    public string Alias
    {
        get => _alias;
        set => SetPropertyValueAndDetectChanges(value, ref _alias!, nameof(Alias));
    }

    /// <summary>
    ///     The CSS value for the selector
    /// </summary>
    public string Value
    {
        get => _value;
        set => SetPropertyValueAndDetectChanges(value, ref _value!, nameof(Value));
    }
}
