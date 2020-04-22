namespace Umbraco.Net
{
    public class NullSessionIdResolver : ISessionIdResolver
    {
        public string SessionId => null;
    }
}
