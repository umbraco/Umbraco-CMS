using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Configuration
{
    public interface IConfigsFactory
    {
        Configs Create();
    }
}
