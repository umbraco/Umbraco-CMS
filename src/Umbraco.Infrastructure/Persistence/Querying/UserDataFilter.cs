namespace Umbraco.Cms.Infrastructure.Persistence.Querying;

public class UserDataFilter : IUserDataFilter
{
    public ICollection<Guid>? UserKeys { get; set; }

    public ICollection<string>? Groups { get; set; }

    public ICollection<string>? Identifiers { get; set; }
}
