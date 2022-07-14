using Umbraco.Cms.Core.Macros;

namespace Umbraco.Cms.Core.Events;

// Provides information on the macro that caused an error
public class MacroErrorEventArgs : EventArgs
{
    /// <summary>
    ///     Name of the faulting macro.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Alias of the faulting macro.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    ///     Filename, file path, fully qualified class name, or other key used by the macro engine to do it's processing of the
    ///     faulting macro.
    /// </summary>
    public string? MacroSource { get; set; }

    /// <summary>
    ///     Exception raised.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    ///     Gets or sets the desired behaviour when a matching macro causes an error. See
    ///     <see cref="MacroErrorBehaviour" /> for definitions. By setting this in your event
    ///     you can override the default behaviour defined in UmbracoSettings.config.
    /// </summary>
    /// <value>Macro error behaviour enum.</value>
    public MacroErrorBehaviour Behaviour { get; set; }

    /// <summary>
    ///     The HTML code to display when Behavior is Content.
    /// </summary>
    public string? Html { get; set; }
}
