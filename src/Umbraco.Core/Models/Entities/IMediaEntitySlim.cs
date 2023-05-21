namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Represents a lightweight media entity, managed by the entity service.
/// </summary>
public interface IMediaEntitySlim : IContentEntitySlim
{
    /// <summary>
    ///     The media file's path/URL
    /// </summary>
    string? MediaPath { get; }
}
