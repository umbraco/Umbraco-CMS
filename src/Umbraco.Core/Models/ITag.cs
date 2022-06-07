using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a tag entity.
/// </summary>
public interface ITag : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or sets the tag group.
    /// </summary>
    [DataMember]
    string Group { get; set; }

    /// <summary>
    ///     Gets or sets the tag text.
    /// </summary>
    [DataMember]
    string Text { get; set; }

    /// <summary>
    ///     Gets or sets the tag language.
    /// </summary>
    [DataMember]
    int? LanguageId { get; set; }

    /// <summary>
    ///     Gets the number of nodes tagged with this tag.
    /// </summary>
    /// <remarks>Only when returning from queries.</remarks>
    int NodeCount { get; }
}
