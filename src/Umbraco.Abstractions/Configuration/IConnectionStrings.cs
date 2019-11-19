namespace Umbraco.Core.Configuration
{
    public interface IConnectionStrings
    {
        ConfigConnectionString this[string key]
        {
            get;
        }
    }
}
