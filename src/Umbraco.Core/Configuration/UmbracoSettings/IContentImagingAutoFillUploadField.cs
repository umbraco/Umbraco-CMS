namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentImagingAutoFillUploadField
    {
        /// <summary>
        /// Allow setting internally so we can create a default
        /// </summary>
        string Alias { get; }

        string WidthFieldAlias { get; }

        string HeightFieldAlias { get; }

        string LengthFieldAlias { get; }

        string ExtensionFieldAlias { get; }
    }
}