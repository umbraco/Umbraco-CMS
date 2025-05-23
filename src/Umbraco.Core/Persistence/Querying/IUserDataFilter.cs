namespace Umbraco.Cms.Infrastructure.Persistence.Querying;

public interface IUserDataFilter
{
    public ICollection<Guid>? UserKeys { get; set; }

    public ICollection<string>? Groups { get; set; }

    public ICollection<string>? Identifiers { get; set; }
}
