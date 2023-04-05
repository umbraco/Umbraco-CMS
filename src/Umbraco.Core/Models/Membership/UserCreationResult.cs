namespace Umbraco.Cms.Core.Models.Membership;

public class UserCreationResult : ErrorMessageResult
{
    public IUser? CreatedUser { get; init; }

    public string? InitialPassword { get; init; }
}
