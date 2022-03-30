namespace Umbraco.Cms.Infrastructure.Runtime
{
    public interface IRuntimeModeValidationService
    {
        bool Validate(out string validationErrorMessage);
    }
}
