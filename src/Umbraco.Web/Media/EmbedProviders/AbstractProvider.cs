using Umbraco.Core.Media;

namespace Umbraco.Web.Media.EmbedProviders
{
    public abstract class AbstractProvider : IEmbedProvider
    {
        protected AbstractProvider()
        {
            SupportsDimensions = true;
        }

        public bool SupportsDimensions { get; set; }

        public abstract string GetMarkup(string url, int maxWidth, int maxHeight);
    }
}