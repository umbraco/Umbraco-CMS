namespace Umbraco.Cms.Core.Headless;

public interface IHeadlessContent : IHeadlessElement
{
    string? Name { get; }

    string Url { get; }
}
