using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Macros;

/// <summary>
///     Renders a macro
/// </summary>
public interface IMacroRenderer
{
    Task<MacroContent> RenderAsync(string macroAlias, IPublishedContent? content, IDictionary<string, object?>? macroParams);
}
