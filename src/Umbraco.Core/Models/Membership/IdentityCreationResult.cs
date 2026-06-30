namespace Umbraco.Cms.Core.Models.Membership;

public class IdentityCreationResult
{
    public static IdentityCreationResult Fail(string errorMessage) =>
        new IdentityCreationResult { ErrorMessage = errorMessage, Succeded = false };

    public static IdentityCreationResult CancelledByNotification() =>
        new IdentityCreationResult { Succeded = false, WasCancelledByNotification = true };

    public bool Succeded { get; init; }

    public bool WasCancelledByNotification { get; init; }

    public string? ErrorMessage { get; init; }

    public string? InitialPassword { get; init; }
}
