namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IRequestHandler
    {
        bool UseDomainPrefixes { get; }

        bool AddTrailingSlash { get; }

        IUrlReplacing UrlReplacing { get; }
    }
}