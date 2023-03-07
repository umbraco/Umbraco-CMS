namespace Umbraco.Cms.Core.Models.Membership;

public class UserCreationResult
{
    public IUser? CreatedUser { get; init; }

    public string? GeneratedPassword { get; init; }

    public string? ErrorMessage { get; init; }
}
