using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IMacroService
    {
        IMacro GetByAlias(string alias);
    }
}
