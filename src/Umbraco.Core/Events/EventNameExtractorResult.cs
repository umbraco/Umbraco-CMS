using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Events
{
    [UmbracoVolatile]
    public class EventNameExtractorResult
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
