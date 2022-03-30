using System.Diagnostics;
using System.Reflection;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators
{
    internal class JITOptimizerValidator : RuntimeModeProductionValidatorBase
    {
        protected override bool Validate(out string validationErrorMessage)
        {
            var debuggableAttribute = Assembly.GetEntryAssembly()?.GetCustomAttribute<DebuggableAttribute>();
            if (debuggableAttribute != null && debuggableAttribute.IsJITOptimizerDisabled)
            {
                validationErrorMessage = "The JIT/runtime optimizer of the entry assembly needs to be enabled in production mode.";
                return false;
            }

            validationErrorMessage = null;
            return true;
        }
    }
}
