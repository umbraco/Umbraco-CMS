namespace Umbraco.Cms.Web.BackOffice.Security;

[Obsolete("Not used anymore")]
public class NoopBackOfficeTwoFactorOptions : IBackOfficeTwoFactorOptions
{
    public string? GetTwoFactorView(string username) => null;
}
