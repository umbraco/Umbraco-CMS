namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Implements <see cref="IMediaEntitySlim" />.
/// </summary>
public class MediaEntitySlim : ContentEntitySlim, IMediaEntitySlim
{
    /// <inheritdoc />
    public string? MediaPath { get; set; }
}
