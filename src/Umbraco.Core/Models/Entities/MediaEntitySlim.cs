namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Implements <see cref="IMediaEntitySlim" />.
/// </summary>
public class MediaEntitySlim : ContentEntitySlim, IMediaEntitySlim
{
    private static readonly IReadOnlyDictionary<string, string> EmptyCultureNames = new Dictionary<string, string>();

    private IReadOnlyDictionary<string, string>? _cultureNames;

    /// <inheritdoc />
    public string? MediaPath { get; set; }

    /// <inheritdoc />
    public ContentVariation Variations { get; set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> CultureNames
    {
        get => _cultureNames ?? EmptyCultureNames;
        set => _cultureNames = value;
    }
}
