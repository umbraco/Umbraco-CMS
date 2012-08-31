using Umbraco.Core.Embed;

namespace Umbraco.Web.Media.EmbedProviders
{
    public abstract class AbstractProvider : IEmbedProvider
    {
        public virtual bool SupportsDimensions
        {
            get { return true; }
        }

        public abstract string GetMarkup(string url, int maxWidth, int maxHeight);

        public virtual string GetPreview(string url, int maxWidth, int maxHeight)
        {
            return GetMarkup(url, maxWidth, maxHeight);
        }
    }
}