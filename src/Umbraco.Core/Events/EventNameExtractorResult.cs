namespace Umbraco.Cms.Core.Events;

public class EventNameExtractorResult
{
    public EventNameExtractorResult(string? name) => Name = name;

    public EventNameExtractorResult(EventNameExtractorError? error) => Error = error;

    public EventNameExtractorError? Error { get; }

    public string? Name { get; }
}
