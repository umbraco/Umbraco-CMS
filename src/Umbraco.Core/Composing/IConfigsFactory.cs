using Umbraco.Core.Configuration;

namespace Umbraco.Core.Composing
{
    public interface IConfigsFactory
    {
        Configs Create();
    }
}
