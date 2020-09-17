namespace Umbraco.Web.PublishedCache
{
    public enum RoutingOutcome
    {
        NotFound = 0,
        Found = 1
    }
    public struct ContentRoutingResult
    {
        public int Id { get; set; }

        public RoutingOutcome Outcome { get; set; }
    }
}
