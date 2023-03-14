namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Implements <see cref="IMediaEntitySlim" />.
/// </summary>
public class MediaEntitySlim : ContentEntitySlim, IMediaEntitySlim
{
    public string? MediaPath { get; set; }
}
