using Umbraco.Cms.Core.Configuration.UmbracoSettings;

namespace Umbraco.Cms.Core.Configuration.Models;

public class CharItem : IChar
{
    /// <summary>
    ///     The character to replace
    /// </summary>
    public string Char { get; set; } = null!;

    /// <summary>
    ///     The replacement character
    /// </summary>
    public string Replacement { get; set; } = null!;
}
