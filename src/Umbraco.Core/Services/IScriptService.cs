using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IScriptService : IBasicFileService<IScript>
{
    Task<Attempt<IScript?, ScriptOperationStatus>> CreateAsync(ScriptCreateModel createModel, Guid performingUserKey);

    Task<Attempt<IScript?, ScriptOperationStatus>> UpdateAsync(ScriptUpdateModel updateModel, Guid performingUserKey);

    Task<ScriptOperationStatus> DeleteAsync(string path, Guid performingUserKey);
}
