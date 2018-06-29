using Umbraco.Core.Media;

namespace Umbraco.Web.Media.EmbedProviders
{
    public abstract class AbstractProvider : IEmbedProvider
    {
        public virtual bool SupportsDimensions
        {
            get { return true; }
        }

        public abstract string GetMarkup(string url, int maxWidth, int maxHeight);
    }
}
