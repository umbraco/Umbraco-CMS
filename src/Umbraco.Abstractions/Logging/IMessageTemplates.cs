namespace Umbraco.Core.Logging
{
    public interface IMessageTemplates
    {
        string Render(string messageTemplate, params object[] args);
    }
}