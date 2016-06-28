namespace Umbraco.Core.Media
{
    public interface IEmbedProvider
    {
        bool SupportsDimensions { get; }

        string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0);
    }
}