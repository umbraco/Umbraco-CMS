namespace Umbraco.Core.Services
{
    public interface IKeyValueService
    {
        string GetValue(string key);
        void SetValue(string key, string value);
        void SetValue(string key, string originValue, string newValue);
    }
}
