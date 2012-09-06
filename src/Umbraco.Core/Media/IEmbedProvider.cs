namespace Umbraco.Core.Media
{
    internal interface IEmbedProvider
    {
        bool SupportsDimensions { get; }

        string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0);
    }
}