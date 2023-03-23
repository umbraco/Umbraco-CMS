namespace Umbraco.Cms.Core.ContentApi;

public interface IRequestStartNodeService
{
    /// <summary>
    ///     Gets the start node path from the "start-node" header, if present.
    /// </summary>
    string? GetRequestedStartNodePath();
}
