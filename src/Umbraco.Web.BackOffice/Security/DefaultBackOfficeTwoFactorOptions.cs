namespace Umbraco.Cms.Web.BackOffice.Security;

public class DefaultBackOfficeTwoFactorOptions : IBackOfficeTwoFactorOptions
{
    public string GetTwoFactorView(string username) => "views\\common\\login-2fa.html";
}
