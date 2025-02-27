namespace Umbraco.Cms.Core.Models.Membership;

public class IdentityCreationResult
{
    public static IdentityCreationResult Fail(string errorMessage) =>
        new IdentityCreationResult { ErrorMessage = errorMessage, Succeded = false };

    public bool Succeded { get; init; }

    public string? ErrorMessage { get; init; }

    public string? InitialPassword { get; init; }
}
