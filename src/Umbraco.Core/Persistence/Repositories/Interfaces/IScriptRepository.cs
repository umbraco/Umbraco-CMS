using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IScriptRepository : IRepository<string, Script>
    {
        bool ValidateScript(Script script);
    }
}