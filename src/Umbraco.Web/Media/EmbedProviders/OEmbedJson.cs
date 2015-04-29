namespace Umbraco.Web.Media.EmbedProviders
{
    public class OEmbedJson : AbstractOEmbedProvider
    {
        public override string GetMarkup(string url, int maxWidth, int maxHeight)
        {
            string requestUrl = BuildFullUrl(url, maxWidth, maxHeight);

            var jsonResponse = GetJsonResponse<OEmbedResponse>(requestUrl);
            return jsonResponse.GetHtml();
        }
    }
}
