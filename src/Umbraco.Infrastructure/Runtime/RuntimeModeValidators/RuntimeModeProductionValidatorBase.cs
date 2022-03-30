using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators
{
    public abstract class RuntimeModeProductionValidatorBase : IRuntimeModeValidator
    {
        public bool Validate(RuntimeMode runtimeMode, out string validationErrorMessage)
        {
            if (runtimeMode == RuntimeMode.Production)
            {
                return Validate(out validationErrorMessage);
            }

            validationErrorMessage = null;
            return true;
        }

        protected abstract bool Validate(out string validationErrorMessage);
    }
}
