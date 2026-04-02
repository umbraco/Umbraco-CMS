namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Provides tools to support message templates.
/// </summary>
public interface IMessageTemplates
{
    /// <summary>
    ///     Renders a message template with the provided arguments.
    /// </summary>
    /// <param name="messageTemplate">The message template containing placeholders.</param>
    /// <param name="args">The arguments to substitute into the template placeholders.</param>
    /// <returns>The rendered message with placeholders replaced by argument values.</returns>
    string Render(string messageTemplate, params object[] args);
}
