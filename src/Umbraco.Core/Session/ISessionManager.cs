namespace Umbraco.Core.Session
{
    public interface ISessionManager
    {
        string GetSessionValue(string sessionName);
        void SetSessionValue(string sessionName, string value);
    }
}
