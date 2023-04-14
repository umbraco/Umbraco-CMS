using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IScriptService : IService
{
    Task<Attempt<IScript?, ScriptOperationStatus>> CreateAsync(ScriptCreateModel createModel, Guid performingUserKey);

    Task<IScript?> GetAsync(string path);

    Task<ScriptOperationStatus> DeleteAsync(string path, Guid performingUserKey);
}
