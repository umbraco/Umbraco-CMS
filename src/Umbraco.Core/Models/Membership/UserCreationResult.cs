namespace Umbraco.Cms.Core.Models.Membership;

public class UserCreationResult : IErrorMessageResult
{
    public IUser? CreatedUser { get; init; }

    public string? InitialPassword { get; init; }

    public string? ErrorMessage { get; init; }
}
