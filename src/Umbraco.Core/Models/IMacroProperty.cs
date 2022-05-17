using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines a Property for a Macro
/// </summary>
public interface IMacroProperty : IValueObject, IDeepCloneable, IRememberBeingDirty
{
    [DataMember]
    int Id { get; set; }

    [DataMember]
    Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the Alias of the Property
    /// </summary>
    [DataMember]
    string Alias { get; set; }

    /// <summary>
    ///     Gets or sets the Name of the Property
    /// </summary>
    [DataMember]
    string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the Sort Order of the Property
    /// </summary>
    [DataMember]
    int SortOrder { get; set; }

    /// <summary>
    ///     Gets or sets the parameter editor alias
    /// </summary>
    [DataMember]
    string EditorAlias { get; set; }
}
