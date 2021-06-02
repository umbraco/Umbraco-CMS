namespace Umbraco.Web.PublishedCache
{
    
    public struct ContentRoutingResult
    {
        public ContentRoutingResult(RoutingOutcome outcome, int id = 0)
        {
            Outcome = outcome;
            Id = id;
        }
        public int Id { get; }

        public RoutingOutcome Outcome { get;  }
    }
}
