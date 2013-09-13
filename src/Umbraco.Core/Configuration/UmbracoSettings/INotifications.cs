namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface INotifications
    {
        string EmailAddress { get; }
        bool DisableHtmlEmail { get; }
    }
}