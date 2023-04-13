using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IScriptService
{
    Task<Attempt<IScript, ScriptOperationStatus>> CreateAsync(ScriptCreateModel createModel, Guid performingUserKey);
}
