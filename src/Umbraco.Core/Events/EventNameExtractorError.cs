using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Events
{
    [UmbracoVolatile]
    public enum EventNameExtractorError
    {
        NoneFound,
        Ambiguous
    }
}
