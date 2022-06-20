namespace Umbraco.Cms.Core.Macros;

public enum MacroErrorBehaviour
{
    /// <summary>
    ///     Default umbraco behavior - show an inline error within the
    ///     macro but allow the page to continue rendering.
    /// </summary>
    Inline,

    /// <summary>
    ///     Silently eat the error and do not display the offending macro.
    /// </summary>
    Silent,

    /// <summary>
    ///     Throw an exception which can be caught by the global error handler
    ///     defined in Application_OnError. If no such error handler is defined
    ///     then you'll see the Yellow Screen Of Death (YSOD) error page.
    /// </summary>
    Throw,

    /// <summary>
    ///     Silently eat the error and display the custom content reported in
    ///     the error event args
    /// </summary>
    Content,
}
