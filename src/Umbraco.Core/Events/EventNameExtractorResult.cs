namespace Umbraco.Core.Events
{
    internal class EventNameExtractorResult
    {
        public EventNameExtractorError? Error { get; private set; }
        public string Name { get; private set; }

        public EventNameExtractorResult(string name)
        {
            Name = name;
        }

        public EventNameExtractorResult(EventNameExtractorError error)
        {
            Error = error;
        }
    }
}
