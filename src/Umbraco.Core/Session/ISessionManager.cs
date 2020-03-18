namespace Umbraco.Core.Session
{
    public interface ISessionManager
    {
        object GetSessionValue(string sessionName);
        void SetSessionValue(string sessionName, object value);
    }
}
