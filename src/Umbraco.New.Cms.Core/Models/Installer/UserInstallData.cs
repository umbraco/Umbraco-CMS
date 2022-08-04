namespace Umbraco.New.Cms.Core.Models.Installer;

public class UserInstallData
{
    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool SubscribeToNewsletter { get; set; }
}
