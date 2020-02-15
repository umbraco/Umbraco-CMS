namespace Umbraco.Infrastructure.Persistence.Repositories
{
    public interface IKeyValueRepository
    {
        void Initialize();

        string GetValue(string key);

        void SetValue(string key, string value);

        bool TrySetValue(string key, string originalValue, string newValue);
    }
}
