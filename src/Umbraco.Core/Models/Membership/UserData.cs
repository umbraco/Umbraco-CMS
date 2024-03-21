namespace Umbraco.Cms.Core.Models.Membership;

public class UserData : IUserData
{
    public Guid Key { get; set; }

    public Guid UserKey { get; set; }

    public string Group { get; set; } = string.Empty;

    public string Identifier { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
