namespace Umbraco.Cms.Core.Models.Membership;

public interface IUserData
{
    public Guid Key { get; set; }

    public Guid UserKey { get; set; }

    public string Group { get; set; }

    public string Identifier { get; set; }

    public string Value { get; set; }
}
