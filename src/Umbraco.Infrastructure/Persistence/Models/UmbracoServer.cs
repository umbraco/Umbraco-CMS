namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoServer
{
    public int Id { get; set; }

    public string Address { get; set; } = null!;

    public string ComputerName { get; set; } = null!;

    public DateTime RegisteredDate { get; set; }

    public DateTime LastNotifiedDate { get; set; }

    public bool IsActive { get; set; }

    public bool IsSchedulingPublisher { get; set; }
}
