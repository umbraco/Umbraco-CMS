namespace Umbraco.New.Cms.Core.Models.Installer;

public class DatabaseInstallData
{
    public Guid Id { get; set; }

    public string? ProviderName { get; set; }

    public string? Server { get; set; }

    public string? Name { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool UseIntegratedAuthentication { get; set; }

    public string? ConnectionString { get; set; }
}
