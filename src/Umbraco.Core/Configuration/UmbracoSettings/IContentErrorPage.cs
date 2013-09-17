namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentErrorPage
    {
        int ContentId { get; }
        string Culture { get; set; }
    }
}