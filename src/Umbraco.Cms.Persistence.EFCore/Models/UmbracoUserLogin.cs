namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoUserLogin
{
    public Guid SessionId { get; set; }

    public int UserId { get; set; }

    public DateTime LoggedInUtc { get; set; }

    public DateTime LastValidatedUtc { get; set; }

    public DateTime? LoggedOutUtc { get; set; }

    public string? IpAddress { get; set; }

    public virtual UmbracoUser User { get; set; } = null!;
}
