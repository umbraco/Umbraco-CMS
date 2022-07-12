namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Provides tools to support message templates.
/// </summary>
public interface IMessageTemplates
{
    string Render(string messageTemplate, params object[] args);
}
