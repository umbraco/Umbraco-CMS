namespace Umbraco.Core.Media
{

    //TODO: Could definitely have done with a better name
    public class Result
    {
        public Status Status { get; set; }
        public bool SupportsDimensions { get; set; }
        public string Markup { get; set; }
    }
}