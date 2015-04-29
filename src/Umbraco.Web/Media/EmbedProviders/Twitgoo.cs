using HtmlAgilityPack;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class Twitgoo : AbstractProvider
    {
        public override bool SupportsDimensions
        {
            get { return false; }
        }

        public override string GetMarkup(string url, int maxWidth, int maxHeight)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);

            var img = doc.DocumentNode.SelectSingleNode("//img [@id = 'fullsize']").Attributes["src"];

            return string.Format("<img src=\"{0}\"/>",
               img.Value);
        }
    }
}