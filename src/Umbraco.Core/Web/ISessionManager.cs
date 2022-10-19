namespace Umbraco.Cms.Core.Web;

public interface ISessionManager
{
    string? GetSessionValue(string key);

    void SetSessionValue(string key, string value);

    void ClearSessionValue(string key);
}
