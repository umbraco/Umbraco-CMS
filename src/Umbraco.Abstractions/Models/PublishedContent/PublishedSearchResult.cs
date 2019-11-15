namespace Umbraco.Core.Models.PublishedContent
{
    public class PublishedSearchResult
    {
        public PublishedSearchResult(IPublishedContent content, float score)
        {
            Content = content;
            Score = score;
        }

        public IPublishedContent Content { get; }
        public float Score { get; }
    }
}
