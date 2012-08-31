using System;

namespace Umbraco.Core.Embed
{
    public interface IEmbedProvider
    {
        bool SupportsDimensions { get; }

        string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0);

        string GetPreview(string url, int maxWidth = 0, int maxHeight = 0);
    }
}