namespace Umbraco.Cms.Core.ContentApi;

public class NoopRequestStartNodeService : IRequestStartNodeService
{
    public string? GetRequestedStartNodePath() => null;
}
