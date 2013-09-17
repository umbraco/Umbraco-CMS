namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ILink
    {
        string Application { get; }

        string ApplicationUrl { get; }

        string Language { get; }

        string UserType { get; }

        string HelpUrl { get; }
    }
}