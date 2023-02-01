namespace Umbraco.Cms.Core.ContentApi;

public interface IStartNodeService
{
    /// <summary>
    ///     Gets the start node from "start-node" header, if present.
    /// </summary>
    string? GetStartNode();
}
