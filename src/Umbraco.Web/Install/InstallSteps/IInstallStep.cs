using System.Collections.Generic;

namespace Umbraco.Web.Install.InstallSteps
{
    internal interface IInstallStep<in T>
    {
        IDictionary<string, object> Setup(T model);
    }
}