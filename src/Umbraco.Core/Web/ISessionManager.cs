namespace Umbraco.Web
{
    public interface ISessionManager
    {
        object GetSessionValue(string sessionName);
        void SetSessionValue(string sessionName, object value);
    }
}
