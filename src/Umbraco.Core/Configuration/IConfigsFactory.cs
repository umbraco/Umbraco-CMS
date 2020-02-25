using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public interface IConfigsFactory
    {
        Configs Create(IIOHelper ioHelper);
    }
}
