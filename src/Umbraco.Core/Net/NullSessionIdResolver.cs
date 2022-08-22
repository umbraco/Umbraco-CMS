namespace Umbraco.Cms.Core.Net;

public class NullSessionIdResolver : ISessionIdResolver
{
    public string? SessionId => null;
}
