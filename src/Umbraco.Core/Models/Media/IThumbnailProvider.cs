namespace Umbraco.Core.Media
{
	public interface IThumbnailProvider
    {
        bool CanProvideThumbnail(string fileUrl);
        string GetThumbnailUrl(string fileUrl);
    }

}
